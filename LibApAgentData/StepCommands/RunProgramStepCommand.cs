using System.Threading;
using System.Threading.Tasks;
using LibApAgentData.Domain;
using LibToolActions.BackgroundTasks;
using Microsoft.Extensions.Logging;
using SystemToolsShared;

namespace LibApAgentData.StepCommands;

public sealed class RunProgramStepCommand : ProcessesToolAction
{
    private readonly RunProgramStepParameters _par;
    private readonly bool _useConsole;

    // ReSharper disable once ConvertToPrimaryConstructor
    public RunProgramStepCommand(ILogger logger, bool useConsole, int procLineId, RunProgramStepParameters par) : base(
        logger, null, null, null, "Run Program", procLineId)
    {
        _useConsole = useConsole;
        _par = par;
    }

    protected override Task<bool> RunAction(CancellationToken cancellationToken)
    {
        StShared.RunProcessWithOutput(_useConsole, Logger, _par.Program, _par.Arguments);
        return Task.FromResult(true);
    }
}