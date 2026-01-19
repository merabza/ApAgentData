using System.Threading;
using System.Threading.Tasks;
using ApAgentData.LibApAgentData.Domain;
using ApAgentData.LibApAgentData.Steps;
using ApAgentData.LibApAgentData.ToolActions;
using LanguageExt;
using Microsoft.Extensions.Logging;
using SystemTools.SystemToolsShared.Errors;
using ToolsManagement.DatabasesManagement;
using ToolsManagement.LibToolActions.BackgroundTasks;

namespace ApAgentData.LibApAgentData.StepCommands;

public sealed class CheckRepairDatabaseStepCommand : MultiDatabaseProcessesToolAction
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public CheckRepairDatabaseStepCommand(ILogger logger, bool useConsole, string procLogFilesFolder,
        ProcessManager processManager, MultiDatabaseProcessStep multiDatabaseProcessStep,
        MultiDatabaseProcessStepParameters par, int procLineId) : base(logger, useConsole, procLogFilesFolder,
        "CheckRepairDataBase", processManager, multiDatabaseProcessStep, par, "Check Repair DataBase", procLineId)
    {
    }

    protected override async Task<bool> RunOneDatabaseAction(IDatabaseManager agentClient, string databaseName,
        CancellationToken cancellationToken = default)
    {
        Option<Err[]> checkRepairDatabaseResult =
            await agentClient.CheckRepairDatabase(databaseName, cancellationToken);
        if (!checkRepairDatabaseResult.IsSome)
        {
            return true;
        }

        Err.PrintErrorsOnConsole((Err[])checkRepairDatabaseResult);
        return false;
    }
}
