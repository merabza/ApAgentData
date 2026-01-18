using System.Net.Http;
using ApAgentData.LibApAgentData.Models;
using ApAgentData.LibApAgentData.StepCommands;
using LibToolActions.BackgroundTasks;
using Microsoft.Extensions.Logging;
using SystemToolsShared;

namespace ApAgentData.LibApAgentData.Steps;

public sealed class UnZipOnPlaceStep : JobStep
{
    public string? PathWithZips { get; set; }
    public bool WithSubFolders { get; set; }

    public override ProcessesToolAction? GetToolAction(ILogger logger, IHttpClientFactory httpClientFactory,
        bool useConsole, ProcessManager processManager, ApAgentParameters parameters, string procLogFilesFolder)
    {
        if (!string.IsNullOrWhiteSpace(PathWithZips))
        {
            return new UnZipOnPlaceCommand(logger, useConsole, processManager, PathWithZips, WithSubFolders,
                ProcLineId);
        }

        StShared.WriteErrorLine("PathWithZips is empty. PathWithZips sis not run", useConsole, logger);
        return null;
    }
}
