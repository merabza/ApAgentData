using LibApAgentData.Domain;
using LibApAgentData.Steps;
using LibToolActions.BackgroundTasks;
using Microsoft.Extensions.Logging;

namespace LibApAgentData.StepCommands;

public sealed class ExecuteSqlCommandStepCommand : ProcessesToolAction
{
    private readonly ExecuteSqlCommandStep _executeSqlCommandStep;

    private readonly ExecuteSqlCommandStepParameters _par;

    public ExecuteSqlCommandStepCommand(ILogger logger, bool useConsole, ProcessManager processManager,
        ExecuteSqlCommandStep executeSqlCommandStep, ExecuteSqlCommandStepParameters par) : base(logger, useConsole,
        processManager, "Execute Sql Command", executeSqlCommandStep.ProcLineId)
    {
        _executeSqlCommandStep = executeSqlCommandStep;
        _par = par;
    }

    protected override bool RunAction()
    {
        return _par.AgentClient.ExecuteCommand(_par.ExecuteQueryCommand, _executeSqlCommandStep.DatabaseName).Result;
    }
}