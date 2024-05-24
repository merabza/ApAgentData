using System.Threading;
using System.Threading.Tasks;
using DatabasesManagement;
using LibApAgentData.Domain;
using LibApAgentData.Steps;
using LibApAgentData.ToolActions;
using LibToolActions.BackgroundTasks;
using Microsoft.Extensions.Logging;
using SystemToolsShared;

namespace LibApAgentData.StepCommands;

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
        CancellationToken cancellationToken)
    {
        var updateStatisticsResult = await agentClient.UpdateStatistics(databaseName, cancellationToken);
        if (!updateStatisticsResult.IsSome)
            return true;

        Err.PrintErrorsOnConsole((Err[])updateStatisticsResult);
        return false;
    }
}