using System.Collections.Generic;
using System.Linq;
using DatabaseManagementClients;
using LibApAgentData.Domain;
using LibApAgentData.Models;
using LibApAgentData.Steps;
using LibToolActions.BackgroundTasks;
using Microsoft.Extensions.Logging;

namespace LibApAgentData.ToolActions;

public /*open*/ class MultiDatabaseProcessesToolAction : ProcessesToolAction
{
    private readonly MultiDatabaseProcessStep _multiDatabaseProcessStep;
    private readonly MultiDatabaseProcessStepParameters _par;
    private readonly string _procLogFilesFolder;
    private readonly string _stepName;

    protected MultiDatabaseProcessesToolAction(ILogger logger, bool useConsole, string procLogFilesFolder,
        string stepName, ProcessManager processManager, MultiDatabaseProcessStep multiDatabaseProcessStep,
        MultiDatabaseProcessStepParameters par, string actionName, int procLineId) : base(logger, useConsole,
        processManager, actionName, procLineId)
    {
        _procLogFilesFolder = procLogFilesFolder;
        _stepName = stepName;
        _multiDatabaseProcessStep = multiDatabaseProcessStep;
        _par = par;
    }


    private List<string> GetDatabaseNames()
    {
        List<string> databaseNames;
        if (_multiDatabaseProcessStep.DatabaseSet == EDatabaseSet.DatabasesBySelection)
        {
            databaseNames = _multiDatabaseProcessStep.DatabaseNames;
        }
        else
        {
            DatabasesListCreator databasesListCreator = new(_multiDatabaseProcessStep.DatabaseSet, _par.AgentClient);

            var dbList = databasesListCreator.LoadDatabaseNames();
            databaseNames = dbList.Select(s => s.Name).ToList();
        }

        return databaseNames;
    }


    protected override bool RunAction()
    {
        if (string.IsNullOrWhiteSpace(_procLogFilesFolder))
        {
            Logger.LogError("Process log files Folder not specified");
            return false;
        }

        var databaseNames = GetDatabaseNames();
        var all = true;
        foreach (var databaseName in databaseNames)
        {
            ProcLogFile procLogFile = new(UseConsole, Logger, $"{_stepName}_{databaseName}_",
                _multiDatabaseProcessStep.PeriodType, _multiDatabaseProcessStep.StartAt,
                _multiDatabaseProcessStep.HoleStartTime, _multiDatabaseProcessStep.HoleEndTime, _procLogFilesFolder,
                _par.LocalWorkFileManager);

            if (procLogFile.HaveCurrentPeriodFile())
            {
                Logger.LogInformation($"{databaseName} {_stepName} had executed in this period");
                continue;
            }

            Logger.LogInformation(
                $"{_stepName} for database {databaseName}");
            if (!RunOneDatabaseAction(_par.AgentClient, databaseName))
            {
                all = false;
                break;
            }

            //სამუშაო ფოლდერში შეიქმნას ფაილი, რომელიც იქნება იმის ამღნიშვნელი,
            //რომ ეს პროცესი შესრულდა და წარმატებით დასრულდა.
            //ფაილის სახელი უნდა შედგებოდეს პროცედურის სახელისაგან თარიღისა და დროისაგან
            //(როცა პროცესი დასრულდა)
            //ასევე სერვერის სახელი და ბაზის სახელი.
            //გაფართოება log
            procLogFile.CreateNow("Ok");
            //ასევე წაიშალოს ანალოგიური პროცესის მიერ წინათ შექმნილი ფაილები
            procLogFile.DeleteOldFiles();
            Logger.LogInformation("Ok");
        }

        return all;
    }


    protected virtual bool RunOneDatabaseAction(DatabaseManagementClient agentClient, string databaseName)
    {
        return false;
    }
}