﻿using System.Net.Http;
using System.Threading;
using DatabasesManagement;
using LibApiClientParameters;
using LibDatabaseParameters;
using Microsoft.Extensions.Logging;
using SystemToolsShared;

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
        var agentClient = DatabaseAgentClientsFabric.CreateDatabaseManager(useConsole, logger, httpClientFactory,
            webAgentName, apiClients, databaseServerConnectionName, databaseServerConnections, null, null,
            CancellationToken.None).Result;

        if (agentClient is null)
        {
            StShared.WriteErrorLine($"DatabaseManagementClient does not created for webAgent {webAgentName}",
                useConsole, logger);
            return null;
        }

        if (!string.IsNullOrWhiteSpace(executeQueryCommand))
            return new ExecuteSqlCommandStepParameters(agentClient, executeQueryCommand);

        StShared.WriteErrorLine("executeQueryCommand does not Specified", useConsole, logger);
        return null;
    }
}