﻿using System.Threading;
using System.Threading.Tasks;
using DatabasesManagement;
using LibApAgentData.Domain;
using LibApAgentData.Steps;
using LibApAgentData.ToolActions;
using LibToolActions.BackgroundTasks;
using Microsoft.Extensions.Logging;
using SystemToolsShared.Errors;

namespace LibApAgentData.StepCommands;

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
        var checkRepairDatabaseResult = await agentClient.CheckRepairDatabase(databaseName, cancellationToken);
        if (!checkRepairDatabaseResult.IsSome)
            return true;

        Err.PrintErrorsOnConsole((Err[])checkRepairDatabaseResult);
        return false;
    }
}