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
        Option<Err[]> updateStatisticsResult = await agentClient.UpdateStatistics(databaseName, cancellationToken);
        if (!updateStatisticsResult.IsSome)
        {
            return true;
        }

        Err.PrintErrorsOnConsole((Err[])updateStatisticsResult);
        return false;
    }
}
