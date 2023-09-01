using LibApAgentData.Domain;
using LibApAgentData.Models;
using LibApAgentData.StepCommands;
using LibFileParameters.Models;
using LibToolActions.BackgroundTasks;
using Microsoft.Extensions.Logging;
using SystemToolsShared;

namespace LibApAgentData.Steps;

public sealed class FilesSyncStep : JobStep
{
    //ფაილსაცავის სახელი, რომელიც გამოიყენება რეზერვაციისათვის ბექაპების ასატვირთად
    public string? SourceFileStorageName { get; set; }
    public string? DestinationFileStorageName { get; set; }
    public string? ExcludeSet { get; set; } //გამოსარიცხი ფაილებისა და გზების კომპლექტის სახელი
    public string? DeleteDestinationFilesSet { get; set; } //მიზნის მხარეს წინასწარ წასაშლელი ფაილები კომპლექტის სახელი
    public string? ReplacePairsSet { get; set; } //აკრძალული თანმიმდევრობის ჩანაცვლების კომპლექტის სახელი

    public override ProcessesToolAction? GetToolAction(ILogger logger, bool useConsole, ProcessManager processManager,
        ApAgentParameters parameters, string procLogFilesFolder)
    {
        var filesSyncStepParameters = FilesSyncStepParameters.Create(logger, useConsole,
            SourceFileStorageName, DestinationFileStorageName, ExcludeSet, DeleteDestinationFilesSet, ReplacePairsSet,
            parameters.GetUploadFileTempExtension(), parameters.GetDownloadFileTempExtension(),
            new FileStorages(parameters.FileStorages), new ExcludeSets(parameters.ExcludeSets),
            new ReplacePairsSets(parameters.ReplacePairsSets));

        if (filesSyncStepParameters is null)
        {
            StShared.WriteErrorLine("filesSyncStepParameters does not created for Files Sync step", useConsole, logger);
            return null;
        }

        return new FilesSyncStepCommand(logger, processManager, this, filesSyncStepParameters);
    }
}