using LibApAgentData.Models;
using LibApAgentData.StepCommands;
using LibToolActions.BackgroundTasks;
using Microsoft.Extensions.Logging;
using SystemToolsShared;

namespace LibApAgentData.Steps;

public sealed class UnZipOnPlaceStep : JobStep
{
    public string? PathWithZips { get; set; }
    public bool WithSubFolders { get; set; }

    public override ProcessesToolAction? GetToolAction(ILogger logger, bool useConsole,
        ProcessManager processManager, ApAgentParameters parameters, string procLogFilesFolder)
    {
        if (string.IsNullOrWhiteSpace(PathWithZips))
        {
            StShared.WriteErrorLine("PathWithZips is empty. PathWithZips sis not run", useConsole, logger);
            return null;
        }

        return new UnZipOnPlaceCommand(logger, useConsole, processManager, PathWithZips, WithSubFolders, ProcLineId);
    }
}