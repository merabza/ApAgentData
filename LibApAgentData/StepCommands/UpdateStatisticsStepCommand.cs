using System.Threading;
using System.Threading.Tasks;
using ApAgentData.LibApAgentData.Domain;
using ApAgentData.LibApAgentData.Steps;
using ApAgentData.LibApAgentData.ToolActions;
using DatabasesManagement;
using LibToolActions.BackgroundTasks;
using Microsoft.Extensions.Logging;
using SystemToolsShared.Errors;

namespace ApAgentData.LibApAgentData.StepCommands;

public sealed class UpdateStatisticsStepCommand : MultiDatabaseProcessesToolAction
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public UpdateStatisticsStepCommand(ILogger logger, bool useConsole, string procLogFilesFolder,
        ProcessManager processManager, MultiDatabaseProcessStep multiDatabaseProcessStep,
        MultiDatabaseProcessStepParameters par, int procLineId) : base(logger, useConsole, procLogFilesFolder,
        "UpdateStatistics", processManager, multiDatabaseProcessStep, par, "Update Statistics", procLineId)
    {
    }

    protected override async Task<bool> RunOneDatabaseAction(IDatabaseManager agentClient, string databaseName,
        CancellationToken cancellationToken = default)
    {
        var updateStatisticsResult = await agentClient.UpdateStatistics(databaseName, cancellationToken);
        if (!updateStatisticsResult.IsSome)
        {
            return true;
        }

        Err.PrintErrorsOnConsole((Err[])updateStatisticsResult);
        return false;
    }
}
