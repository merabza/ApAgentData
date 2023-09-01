using DatabasesManagement;
using LibApAgentData.Domain;
using LibApAgentData.Steps;
using LibApAgentData.ToolActions;
using LibToolActions.BackgroundTasks;
using Microsoft.Extensions.Logging;

namespace LibApAgentData.StepCommands;

public sealed class RecompileProceduresStepCommand : MultiDatabaseProcessesToolAction
{
    public RecompileProceduresStepCommand(ILogger logger, bool useConsole, string procLogFilesFolder,
        ProcessManager processManager, MultiDatabaseProcessStep multiDatabaseProcessStep,
        MultiDatabaseProcessStepParameters par, int procLineId) : base(logger, useConsole, procLogFilesFolder,
        "RecompileProcedures", processManager, multiDatabaseProcessStep, par, "Recompile Procedures", procLineId)
    {
    }

    protected override bool RunOneDatabaseAction(IDatabaseApiClient agentClient, string databaseName)
    {
        return agentClient.RecompileProcedures(databaseName).Result;
    }
}