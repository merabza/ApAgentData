using LibApAgentData.Domain;
using LibToolActions.BackgroundTasks;
using Microsoft.Extensions.Logging;
using SystemToolsShared;

namespace LibApAgentData.StepCommands;

public sealed class RunProgramStepCommand : ProcessesToolAction
{
    private readonly bool _useConsole;
    private readonly RunProgramStepParameters _par;

    public RunProgramStepCommand(ILogger logger, bool useConsole, int procLineId, RunProgramStepParameters par) : base(
        logger, null, null, null, "Run Program", procLineId)
    {
        _useConsole = useConsole;
        _par = par;
    }

    protected override bool RunAction()
    {
        StShared.RunProcessWithOutput(_useConsole, Logger, _par.Program, _par.Arguments);
        return true;
    }
}