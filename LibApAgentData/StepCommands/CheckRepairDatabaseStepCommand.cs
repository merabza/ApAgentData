using DatabasesManagement;
using LibApAgentData.Domain;
using LibApAgentData.Steps;
using LibApAgentData.ToolActions;
using LibToolActions.BackgroundTasks;
using Microsoft.Extensions.Logging;

namespace LibApAgentData.StepCommands;

public sealed class CheckRepairDatabaseStepCommand : MultiDatabaseProcessesToolAction
{
    public CheckRepairDatabaseStepCommand(ILogger logger, bool useConsole, string procLogFilesFolder,
        ProcessManager processManager, MultiDatabaseProcessStep multiDatabaseProcessStep,
        MultiDatabaseProcessStepParameters par, int procLineId) : base(logger, useConsole, procLogFilesFolder,
        "CheckRepairDataBase", processManager, multiDatabaseProcessStep, par, "Check Repair DataBase", procLineId)
    {
    }


    protected override bool RunOneDatabaseAction(IDatabaseApiClient agentClient, string databaseName)
    {
        return agentClient.CheckRepairDatabase(databaseName).Result;
    }
}