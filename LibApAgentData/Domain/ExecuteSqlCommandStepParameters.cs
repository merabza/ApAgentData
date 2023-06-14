using DatabaseApiClients;
using DatabaseManagementClients;
using LibApiClientParameters;
using LibDatabaseParameters;
using Microsoft.Extensions.Logging;
using SystemToolsShared;

namespace LibApAgentData.Domain;

public sealed class ExecuteSqlCommandStepParameters
{
    private ExecuteSqlCommandStepParameters(DatabaseManagementClient agentClient, string executeQueryCommand)
    {
        AgentClient = agentClient;
        ExecuteQueryCommand = executeQueryCommand;
        //DatabaseServerName = databaseServerName;
    }

    public DatabaseManagementClient AgentClient { get; }

    public string ExecuteQueryCommand { get; }
    //public string? DatabaseServerName { get; }

    public static ExecuteSqlCommandStepParameters? Create(ILogger logger, bool useConsole, string? executeQueryCommand,
        string? webAgentName, ApiClients apiClients, string? databaseServerConnectionName,
        DatabaseServerConnections databaseServerConnections)
    {
        var agentClient = DatabaseAgentClientsFabric.CreateDatabaseManagementClient(useConsole,
            logger, webAgentName, apiClients, databaseServerConnectionName, databaseServerConnections);

        if (agentClient is null)
        {
            StShared.WriteErrorLine($"DatabaseManagementClient does not created for webAgent {webAgentName}",
                useConsole, logger);
            return null;
        }

        if (string.IsNullOrWhiteSpace(executeQueryCommand))
        {
            StShared.WriteErrorLine("executeQueryCommand does not Specified", useConsole, logger);
            return null;
        }

        return new ExecuteSqlCommandStepParameters(agentClient, executeQueryCommand);
    }
}