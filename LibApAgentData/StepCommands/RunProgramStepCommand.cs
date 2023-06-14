using LibApAgentData.Domain;
using LibToolActions.BackgroundTasks;
using Microsoft.Extensions.Logging;
using SystemToolsShared;

namespace LibApAgentData.StepCommands;

public sealed class RunProgramStepCommand : ProcessesToolAction
{
    private readonly RunProgramStepParameters _par;

    public RunProgramStepCommand(ILogger logger, bool useConsole, int procLineId, RunProgramStepParameters par) : base(
        logger, useConsole, null, "Run Program", procLineId)
    {
        _par = par;
    }

    protected override bool RunAction()
    {
        StShared.RunProcessWithOutput(UseConsole, Logger, _par.Program, _par.Arguments);
        return true;
    }
}