using System.Net.Http;
using ApAgentData.LibApAgentData.Domain;
using ApAgentData.LibApAgentData.Models;
using ApAgentData.LibApAgentData.StepCommands;
using LibToolActions.BackgroundTasks;
using Microsoft.Extensions.Logging;
using SystemToolsShared;

namespace ApAgentData.LibApAgentData.Steps;

public sealed class RunProgramStep : JobStep
{
    public string? Program { get; set; } //პროგრამა. რომელიც უნდა გაეშვას
    public string? Arguments { get; set; } //პროგრამის არგუმენტები

    public override ProcessesToolAction? GetToolAction(ILogger logger, IHttpClientFactory httpClientFactory,
        bool useConsole, ProcessManager processManager, ApAgentParameters parameters, string procLogFilesFolder)
    {
        var par = RunProgramStepParameters.Create(logger, useConsole, Program, Arguments);

        if (par is not null)
        {
            return new RunProgramStepCommand(logger, useConsole, ProcLineId, par);
        }

        StShared.WriteErrorLine("parameters does not created, RunProgramStep did not run", useConsole, logger);
        return null;
    }
}
