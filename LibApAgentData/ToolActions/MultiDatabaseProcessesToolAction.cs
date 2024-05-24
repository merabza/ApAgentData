using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DatabasesManagement;
using LibApAgentData.Domain;
using LibApAgentData.Models;
using LibApAgentData.Steps;
using LibToolActions.BackgroundTasks;
using Microsoft.Extensions.Logging;

namespace LibApAgentData.ToolActions;

public /*open*/ class MultiDatabaseProcessesToolAction : ProcessesToolAction
{
    private readonly ILogger _logger;
    private readonly MultiDatabaseProcessStep _multiDatabaseProcessStep;
    private readonly MultiDatabaseProcessStepParameters _par;
    private readonly string _procLogFilesFolder;
    private readonly string _stepName;
    private readonly bool _useConsole;

    protected MultiDatabaseProcessesToolAction(ILogger logger, bool useConsole, string procLogFilesFolder,
        string stepName, ProcessManager processManager, MultiDatabaseProcessStep multiDatabaseProcessStep,
        MultiDatabaseProcessStepParameters par, string actionName, int procLineId) : base(logger, null, null,
        processManager, actionName, procLineId)
    {
        _logger = logger;
        _useConsole = useConsole;
        _procLogFilesFolder = procLogFilesFolder;
        _stepName = stepName;
        _multiDatabaseProcessStep = multiDatabaseProcessStep;
        _par = par;
    }


    private async Task<List<string>> GetDatabaseNames(CancellationToken cancellationToken)
    {
        List<string> databaseNames;
        if (_multiDatabaseProcessStep.DatabaseSet == EDatabaseSet.DatabasesBySelection)
        {
            databaseNames = _multiDatabaseProcessStep.DatabaseNames;
        }
        else
        {
            DatabasesListCreator databasesListCreator = new(_multiDatabaseProcessStep.DatabaseSet, _par.AgentClient);

            var dbList = await databasesListCreator.LoadDatabaseNames(cancellationToken);
            databaseNames = dbList.Select(s => s.Name).ToList();
        }

        return databaseNames;
    }


    protected override async Task<bool> RunAction(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_procLogFilesFolder))
        {
            _logger.LogError("Process log files Folder not specified");
            return false;
        }

        var databaseNames = await GetDatabaseNames(cancellationToken);
        var all = true;
        foreach (var databaseName in databaseNames)
        {
            ProcLogFile procLogFile = new(_useConsole, _logger, $"{_stepName}_{databaseName}_",
                _multiDatabaseProcessStep.PeriodType, _multiDatabaseProcessStep.StartAt,
                _multiDatabaseProcessStep.HoleStartTime, _multiDatabaseProcessStep.HoleEndTime, _procLogFilesFolder,
                _par.LocalWorkFileManager);

            if (procLogFile.HaveCurrentPeriodFile())
            {
                _logger.LogInformation("{databaseName} {_stepName} had executed in this period", databaseName,
                    _stepName);
                continue;
            }

            _logger.LogInformation(
                "{_stepName} for database {databaseName}", _stepName, databaseName);
            if (!await RunOneDatabaseAction(_par.AgentClient, databaseName, cancellationToken))
            {
                all = false;
                break;
            }

            //სამუშაო ფოლდერში შეიქმნას ფაილი, რომელიც იქნება იმის აღმნიშვნელი,
            //რომ ეს პროცესი შესრულდა და წარმატებით დასრულდა.
            //ფაილის სახელი უნდა შედგებოდეს პროცედურის სახელისაგან თარიღისა და დროისაგან
            //(როცა პროცესი დასრულდა)
            //ასევე სერვერის სახელი და ბაზის სახელი.
            //გაფართოება log
            procLogFile.CreateNow("Ok");
            //ასევე წაიშალოს ანალოგიური პროცესის მიერ წინათ შექმნილი ფაილები
            procLogFile.DeleteOldFiles();
            _logger.LogInformation("Ok");
        }

        return all;
    }


    protected virtual Task<bool> RunOneDatabaseAction(IDatabaseManager agentClient, string databaseName,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(false);
    }
}