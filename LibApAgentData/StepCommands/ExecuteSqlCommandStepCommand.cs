using System.Threading;
using System.Threading.Tasks;
using ApAgentData.LibApAgentData.Domain;
using ApAgentData.LibApAgentData.Steps;
using LibToolActions.BackgroundTasks;
using Microsoft.Extensions.Logging;
using SystemToolsShared.Errors;

namespace ApAgentData.LibApAgentData.StepCommands;

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

    protected override async ValueTask<bool> RunAction(CancellationToken cancellationToken = default)
    {
        var executeCommandResult = await _par.AgentClient.ExecuteCommand(_par.ExecuteQueryCommand,
            _executeSqlCommandStep.DatabaseName, cancellationToken);
        if (executeCommandResult.IsSome)
        {
            Err.PrintErrorsOnConsole((Err[])executeCommandResult);
        }

        return true;
    }
}
