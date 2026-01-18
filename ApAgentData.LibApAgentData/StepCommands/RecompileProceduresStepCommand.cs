using System.Threading;
using System.Threading.Tasks;
using ApAgentData.LibApAgentData.Domain;
using ApAgentData.LibApAgentData.Steps;
using ApAgentData.LibApAgentData.ToolActions;
using Microsoft.Extensions.Logging;
using SystemTools.SystemToolsShared.Errors;
using ToolsManagement.DatabasesManagement;
using ToolsManagement.LibToolActions.BackgroundTasks;

namespace ApAgentData.LibApAgentData.StepCommands;

public sealed class RecompileProceduresStepCommand : MultiDatabaseProcessesToolAction
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public RecompileProceduresStepCommand(ILogger logger, bool useConsole, string procLogFilesFolder,
        ProcessManager processManager, MultiDatabaseProcessStep multiDatabaseProcessStep,
        MultiDatabaseProcessStepParameters par, int procLineId) : base(logger, useConsole, procLogFilesFolder,
        "RecompileProcedures", processManager, multiDatabaseProcessStep, par, "Recompile Procedures", procLineId)
    {
    }

    protected override async Task<bool> RunOneDatabaseAction(IDatabaseManager agentClient, string databaseName,
        CancellationToken cancellationToken = default)
    {
        var recompileProceduresResult = await agentClient.RecompileProcedures(databaseName, cancellationToken);
        if (!recompileProceduresResult.IsSome)
        {
            return true;
        }

        Err.PrintErrorsOnConsole((Err[])recompileProceduresResult);
        return false;
    }
}
