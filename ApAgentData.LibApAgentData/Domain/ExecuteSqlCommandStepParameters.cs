using System.Net.Http;
using System.Threading;
using Microsoft.Extensions.Logging;
using ParametersManagement.LibApiClientParameters;
using ParametersManagement.LibDatabaseParameters;
using SystemTools.SystemToolsShared;
using SystemTools.SystemToolsShared.Errors;
using ToolsManagement.DatabasesManagement;

namespace ApAgentData.LibApAgentData.Domain;

public sealed class ExecuteSqlCommandStepParameters
{
    private ExecuteSqlCommandStepParameters(IDatabaseManager agentClient, string executeQueryCommand)
    {
        AgentClient = agentClient;
        ExecuteQueryCommand = executeQueryCommand;
    }

    public IDatabaseManager AgentClient { get; }

    public string ExecuteQueryCommand { get; }

    public static ExecuteSqlCommandStepParameters? Create(ILogger logger, IHttpClientFactory httpClientFactory,
        bool useConsole, string? executeQueryCommand, string? webAgentName, ApiClients apiClients,
        string? databaseServerConnectionName, DatabaseServerConnections databaseServerConnections)
    {
        var createDatabaseManagerResult = DatabaseManagersFactory.CreateDatabaseManager(logger, useConsole,
            databaseServerConnectionName, databaseServerConnections, apiClients, httpClientFactory, null, null,
            CancellationToken.None).Result;

        if (createDatabaseManagerResult.IsT1)
        {
            Err.PrintErrorsOnConsole(createDatabaseManagerResult.AsT1);
        }

        if (!string.IsNullOrWhiteSpace(executeQueryCommand))
        {
            return new ExecuteSqlCommandStepParameters(createDatabaseManagerResult.AsT0, executeQueryCommand);
        }

        StShared.WriteErrorLine("executeQueryCommand does not Specified", useConsole, logger);
        return null;
    }
}
