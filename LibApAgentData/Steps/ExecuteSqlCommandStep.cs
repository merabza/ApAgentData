using LibApAgentData.Domain;
using LibApAgentData.Models;
using LibApAgentData.StepCommands;
using LibApiClientParameters;
using LibDatabaseParameters;
using LibToolActions.BackgroundTasks;
using Microsoft.Extensions.Logging;
using SystemToolsShared;

namespace LibApAgentData.Steps;

public sealed class ExecuteSqlCommandStep : JobStep
{
    //თუ ბაზასთან დასაკავშირებლად ვიყენებთ პირდაპირ კავშირს, მაშინ ვებაგენტი აღარ გამოიყენება და პირიქით
    public string? DatabaseServerConnectionName { get; set; } //ბაზასთან დაკავშირების პარამეტრების ჩანაწერის სახელი

    public string? DatabaseWebAgentName { get; set; } //შეიძლება ბაზასთან დასაკავშირებლად გამოვიყენოთ ვებაგენტი
    //public string? DatabaseServerName { get; set; } //გამოიყენება მხოლოდ იმ შემთხვევაში თუ ვიყენებთ WebAgent-ს

    public string? DatabaseName { get; set; } //მონაცემთა ბაზის სახელი

    public string? ExecuteQueryCommand { get; set; } //შესასრულებელი ბრძანების ტექსტი

    //ბრძანების შესრულების ტაიმაუტი. თუ ამ დროში ბრძანება არ დასრულდა. პროცესი უნდა გაჩერდეს.
    public int CommandTimeOut { get; set; }

    public override ProcessesToolAction? GetToolAction(ILogger logger, bool useConsole, ProcessManager processManager,
        ApAgentParameters parameters, string procLogFilesFolder)
    {
        var par = ExecuteSqlCommandStepParameters.Create(logger, useConsole, ExecuteQueryCommand, DatabaseWebAgentName,
            new ApiClients(parameters.ApiClients), DatabaseServerConnectionName,
            new DatabaseServerConnections(parameters.DatabaseServerConnections));

        if (par is not null)
            return new ExecuteSqlCommandStepCommand(logger, useConsole, processManager, this, par);

        StShared.WriteErrorLine("par does not created", useConsole, logger);
        return null;
    }
}