using System.Threading;
using System.Threading.Tasks;
using LibApAgentData.Domain;
using LibApAgentData.FolderProcessors;
using LibApAgentData.Steps;
using LibToolActions.BackgroundTasks;
using Microsoft.Extensions.Logging;

// ReSharper disable ConvertToPrimaryConstructor

namespace LibApAgentData.StepCommands;

public sealed class FilesSyncStepCommand : ProcessesToolAction
{
    private readonly ILogger _logger;
    private readonly FilesSyncStepParameters _par;

    public FilesSyncStepCommand(ILogger logger, ProcessManager processManager, JobStep jobStep,
        FilesSyncStepParameters filesSyncStepParameters) : base(logger, null, null, processManager, "Files Sync",
        jobStep.ProcLineId)
    {
        _logger = logger;
        _par = filesSyncStepParameters;
    }

    protected override Task<bool> RunAction(CancellationToken cancellationToken)
    {
        //სანამ რაიმეს გადაწერას დავიწყებთ, დავრწმუნდეთ, რომ მიზნის მხარეს არ არის შემორჩენილი ძველი დროებითი ფაილები
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
        //მაშინ ბოლოში (გაფართოების წინ) მივაწეროთ ფრჩილებში ჩასმული 2.
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

        var destinationFileMaxLength = _par.DestinationFileStorage.FileNameMaxLength == 0
            ? 255
            : _par.DestinationFileStorage.FileNameMaxLength;

        //თუ წყაროს ფოლდერი ცარელაა, გასაკეთებლი არაფერია
        if (!_par.SourceFileManager.IsFolderEmpty(null))
        {
            PrepareFolderFileNames prepareFolderFileNames = new(_par.SourceFileManager, _par.UseMethod,
                _par.UploadTempExtension, _par.DownloadTempExtension, _par.ExcludeSet, destinationFileMaxLength);

            if (!prepareFolderFileNames.Run())
                return Task.FromResult(false);


            CopyAndReplaceFiles copyAndReplaceFiles = new(_logger, _par.SourceFileManager, _par.DestinationFileManager,
                _par.UseMethod, _par.UploadTempExtension, _par.DownloadTempExtension, _par.ExcludeSet,
                destinationFileMaxLength);

            if (!copyAndReplaceFiles.Run())
                return Task.FromResult(false);
        }

        //თუ მიზნის ფოლდერი ცარელაა, გასაკეთებლი არაფერია
        if (!_par.DestinationFileManager.IsFolderEmpty(null))
        {
            DeleteRedundantFiles deleteRedundantFiles =
                new(_par.SourceFileManager, _par.DestinationFileManager, _par.ExcludeSet);

            if (!deleteRedundantFiles.Run())
                return Task.FromResult(false);
        }

        //ცარელა ფოლდერების წაშლა
        EmptyFoldersRemover emptyFoldersRemover = new(_par.DestinationFileManager);
        return Task.FromResult(emptyFoldersRemover.Run());
    }
}