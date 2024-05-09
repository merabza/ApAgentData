using System;
using System.Threading;
using System.Threading.Tasks;
using LibApAgentData.Domain;
using LibApAgentData.FolderProcessors;
using LibApAgentData.Steps;
using LibApAgentData.SubCommands;
using LibToolActions.BackgroundTasks;
using Microsoft.Extensions.Logging;

namespace LibApAgentData.StepCommands;

public sealed class FilesMoveStepCommand : ProcessesToolAction
{
    private readonly ILogger _logger;
    private readonly FilesMoveStepParameters _par;
    private readonly bool _useConsole;

    // ReSharper disable once ConvertToPrimaryConstructor
    public FilesMoveStepCommand(ILogger logger, bool useConsole, ProcessManager processManager, JobStep jobStep,
        FilesMoveStepParameters filesMoveStepParameters) : base(logger, null, null, processManager, "Files Move",
        jobStep.ProcLineId)
    {
        _logger = logger;
        _useConsole = useConsole;
        _par = filesMoveStepParameters;
    }

    protected override Task<bool> RunAction(CancellationToken cancellationToken)
    {
        //სანამ რაიმეს გადაწერას დავიწყებთ, დავრწმუნდეთ, რომ მიზნის მხარეს არ არის შემორჩენილი ველი დროებითი ფაილები
        if (_par.DeleteDestinationFilesSet != null)
        {
            DeleteTempFiles deleteTempFiles = new(_par.DestinationFileManager,
                [.. _par.DeleteDestinationFilesSet.FolderFileMasks]);

            if (!deleteTempFiles.Run())
                return Task.FromResult(false);
        }

        //თუ მიზანი მოშორებულია და FTP-ა, 
        //სანამ გადაწერას დავიწყებთ, დავრწმუნდეთ, რომ არ არის აკრძალული სახელის ფაილები.
        //ჯერჯერობით რაც ვერ მუშავდება FTP-ს მხარეს არის თანმიმდევრობით სამი წერტილი სახელში 
        //ზოგადი მიდგომა ასეთი უნდა იყოს. ვიპოვოთ ფაილები, რომელთაც აქვთ აკრძალული თანმიმდევრობა სახელში
        //და ჩავანაცვლოთ ახლოს მდგომი დასაშვები ვარიანტით
        //მაგალითად ... -> .
        //ჩანაცვლებისას აღმოჩნდება, რომ ახალი სახელით უკვე არის სხვა ფაილი იმავე ფოლდერში,
        //მაშინ ბოლოში (გაფართოების წინ) მივაწეროთ ფრჩხილებში ჩასმული 2.
        //თუ ასეთიც არის, მაშინ ავიღოთ სამი და ასე მანამ, სანამ არ ვიპოვით თავისუფალ სახელს

        //FTP-ს მხარეს არ აიტვირთოს ისეთი ფაილები, რომლებიც შეიცავენ მიმდევრობით 2 ან მეტ წერტილს.
        //ასეთ შემთხვევაში დავიდეთ 1 წერტილამდე და ისე ავტვირთოთ
        //თუ ასეთი სახელი იარსებებს გამოვიყენოთ ციფრები ბოლოში

        if (_par.ReplacePairsSet != null)
        {
            ChangeFilesWithRestrictPatterns changeFilesWithManyDots =
                new(_par.SourceFileManager, _par.ReplacePairsSet.PairsDict);
            if (!changeFilesWithManyDots.Run())
                return Task.FromResult(false);
        }

        //ლოკალურიდან FTP-ს მხარეს ატვირთვის დროს,
        //ან ლოკალურიდან ლოკალურში გადაადგილებისას,
        //წინასწარ დამუშავდეს ადგილზევე zip ფაილები
        if (_par.SourceIsLocal)
        {
            UnZipOnPlace unZipOnPlace = new(_logger, _useConsole, _par.SourceFileManager);
            if (!unZipOnPlace.Run())
                return Task.FromResult(false);
        }

        //თუ წყაროს ფოლდერი ცარიელია, გასაკეთებლი არაფერია
        if (!_par.SourceFileManager.IsFolderEmpty(null))
        {
            string? moveFolderName = null;
            if (_par.CreateFolderWithDateTime)
            {
                //შევქმნათ ამ სესიის შესაბამისი დროის მიხედვით ფოლდერის სახელი
                moveFolderName = DateTime.Now.ToString(_par.MoveFolderMask);

                //შევამოწმოთ ასატვირთ ფოლდერში თუ არსებობს სესიის შესაბამისი ფოლდერი.
                //თუ არ არსებობს, ვქმნით. //თუ ფოლდერი ვერ შეიქმნა, ვჩერდებით
                if (!_par.DestinationFileManager.CareCreateDirectory(moveFolderName))
                    return Task.FromResult(false);
            }

            MoveFiles moveFiles = new(_logger, _par.SourceFileManager, _par.DestinationFileManager, moveFolderName,
                _par.UseMethod, _par.UploadTempExtension, _par.DownloadTempExtension, _par.ExcludeSet,
                _par.MaxFolderCount,
                _par.DestinationFileStorage.FileNameMaxLength == 0
                    ? 255
                    : _par.DestinationFileStorage.FileNameMaxLength);

            if (!moveFiles.Run())
                return Task.FromResult(false);
        }

        if (!_par.DestinationIsLocal)
            return Task.FromResult(true);

        DuplicateFilesFinder duplicateFilesFinder = new(_par.DestinationFileManager);
        if (!duplicateFilesFinder.Run())
            return Task.FromResult(false);

        MultiDuplicatesFinder multiDuplicatesFinder = new(_useConsole, duplicateFilesFinder.FileList);
        if (!multiDuplicatesFinder.Run())
            return Task.FromResult(false);

        DuplicateFilesRemover duplicateFilesRemover =
            new(_useConsole, multiDuplicatesFinder.FileList, _par.PriorityPoints);

        if (!duplicateFilesRemover.Run())
            return Task.FromResult(false);

        //ცარიელი ფოლდერების წაშლა
        EmptyFoldersRemover emptyFoldersRemover = new(_par.DestinationFileManager);
        return Task.FromResult(emptyFoldersRemover.Run());
    }
}