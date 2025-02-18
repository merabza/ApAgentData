using System.Net.Http;
using System.Threading;
using DatabasesManagement;
using LibApiClientParameters;
using LibDatabaseParameters;
using Microsoft.Extensions.Logging;
using SystemToolsShared;
using SystemToolsShared.Errors;

namespace LibApAgentData.Domain;

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
        var createDatabaseManagerResult = DatabaseManagersFabric.CreateDatabaseManager(logger, useConsole,
            databaseServerConnectionName, databaseServerConnections, apiClients, httpClientFactory, null, null,
            CancellationToken.None).Result;

        if (createDatabaseManagerResult.IsT1) Err.PrintErrorsOnConsole(createDatabaseManagerResult.AsT1);

        if (!string.IsNullOrWhiteSpace(executeQueryCommand))
            return new ExecuteSqlCommandStepParameters(createDatabaseManagerResult.AsT0, executeQueryCommand);

        StShared.WriteErrorLine("executeQueryCommand does not Specified", useConsole, logger);
        return null;
    }
}