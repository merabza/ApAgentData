using System;
using System.Collections.Generic;
using System.Net.Http;
using FileManagersMain;
using LibApAgentData.Domain;
using LibApAgentData.Models;
using LibApAgentData.StepCommands;
using LibApiClientParameters;
using LibDatabaseParameters;
using LibToolActions.BackgroundTasks;
using Microsoft.Extensions.Logging;

namespace LibApAgentData.Steps;

public sealed class MultiDatabaseProcessStep : JobStep
{
    public EMultiDatabaseActionType ActionType { get; set; } //გასაშვები პროცესის ტიპი

    //თუ ბაზასთან დასაკავშირებლად ვიყენებთ პირდაპირ კავშირს, მაშინ ვებაგენტი აღარ გამოიყენება და პირიქით
    public string? DatabaseServerConnectionName { get; set; } //ბაზასთან დაკავშირების პარამეტრების ჩანაწერის სახელი
    public string? DatabaseWebAgentName { get; set; } //შეიძლება ბაზასთან დასაკავშირებლად გამოვიყენოთ ვებაგენტი
    public string? DatabaseServerName { get; set; } //გამოიყენება მხოლოდ იმ შემთხვევაში თუ ვიყენებთ WebAgent-ს

    public EDatabaseSet DatabaseSet { get; set; } //ბაზების სიმრავლე, რომლისთვისაც უნდა გაეშვას ეს პროცესი.

    //თუ DatabaseSet-ის მნიშვნელობაა DatabasesBySelection, მაშინ მონაცემთა ბაზების სახელები უნდა ავიღოთ ქვემოთ მოცემული სიიდან
    public List<string> DatabaseNames { get; set; } = [];

    public override ProcessesToolAction? GetToolAction(ILogger logger, IHttpClientFactory httpClientFactory,
        bool useConsole, ProcessManager processManager, ApAgentParameters parameters, string procLogFilesFolder)
    {
        var localWorkFileManager = FileManagersFactory.CreateFileManager(useConsole, logger, procLogFilesFolder);

        if (localWorkFileManager == null)
        {
            logger.LogError("workFileManager for procLogFilesFolder does not created");
            return null;
        }

        var par = MultiDatabaseProcessStepParameters.Create(logger, httpClientFactory, useConsole,
            new ApiClients(parameters.ApiClients), DatabaseServerConnectionName,
            new DatabaseServerConnections(parameters.DatabaseServerConnections), procLogFilesFolder);

        if (par is not null)
            return ActionType switch
            {
                EMultiDatabaseActionType.UpdateStatistics => new UpdateStatisticsStepCommand(logger, useConsole,
                    procLogFilesFolder, processManager, this, par, ProcLineId),
                EMultiDatabaseActionType.CheckRepairDataBase => new CheckRepairDatabaseStepCommand(logger, useConsole,
                    procLogFilesFolder, processManager, this, par, ProcLineId),
                EMultiDatabaseActionType.RecompileProcedures => new RecompileProceduresStepCommand(logger, useConsole,
                    procLogFilesFolder, processManager, this, par, ProcLineId),
                _ => throw new ArgumentOutOfRangeException()
            };

        logger.LogError("Error when creating MultiDatabaseProcessStep parameters");
        return null;
    }
}