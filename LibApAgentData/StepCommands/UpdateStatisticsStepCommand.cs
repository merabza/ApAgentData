using DatabasesManagement;
using LibApAgentData.Domain;
using LibApAgentData.Steps;
using LibApAgentData.ToolActions;
using LibToolActions.BackgroundTasks;
using Microsoft.Extensions.Logging;

namespace LibApAgentData.StepCommands;

public sealed class UpdateStatisticsStepCommand : MultiDatabaseProcessesToolAction
{
    public UpdateStatisticsStepCommand(ILogger logger, bool useConsole, string procLogFilesFolder,
        ProcessManager processManager, MultiDatabaseProcessStep multiDatabaseProcessStep,
        MultiDatabaseProcessStepParameters par, int procLineId) : base(logger, useConsole, procLogFilesFolder,
        "UpdateStatistics", processManager, multiDatabaseProcessStep, par, "Update Statistics", procLineId)
    {
    }

    protected override bool RunOneDatabaseAction(IDatabaseApiClient agentClient, string databaseName)
    {
        return agentClient.UpdateStatistics(databaseName).Result;
    }
}