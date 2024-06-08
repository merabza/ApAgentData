using LibApAgentData.Domain;
using LibApAgentData.Steps;
using LibToolActions.BackgroundTasks;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using SystemToolsShared.Errors;

namespace LibApAgentData.StepCommands;

public sealed class ExecuteSqlCommandStepCommand : ProcessesToolAction
{
    private readonly ExecuteSqlCommandStep _executeSqlCommandStep;

    private readonly ExecuteSqlCommandStepParameters _par;

    // ReSharper disable once ConvertToPrimaryConstructor
    public ExecuteSqlCommandStepCommand(ILogger logger, ProcessManager processManager,
        ExecuteSqlCommandStep executeSqlCommandStep, ExecuteSqlCommandStepParameters par) : base(logger, null, null,
        processManager, "Execute Sql Command", executeSqlCommandStep.ProcLineId)
    {
        _executeSqlCommandStep = executeSqlCommandStep;
        _par = par;
    }

    protected override async Task<bool> RunAction(CancellationToken cancellationToken)
    {
        var executeCommandResult = await _par.AgentClient.ExecuteCommand(_par.ExecuteQueryCommand, cancellationToken,
            _executeSqlCommandStep.DatabaseName);
        if (executeCommandResult.IsSome)
            Err.PrintErrorsOnConsole((Err[])executeCommandResult);
        return true;
    }
}