using System.Net.Http;
using System.Threading;
using DatabasesManagement;
using FileManagersMain;
using LibApiClientParameters;
using LibDatabaseParameters;
using Microsoft.Extensions.Logging;
using SystemToolsShared;

namespace LibApAgentData.Domain;

public sealed class MultiDatabaseProcessStepParameters
{
    // ReSharper disable once ConvertToPrimaryConstructor
    private MultiDatabaseProcessStepParameters(IDatabaseApiClient agentClient, FileManager localWorkFileManager)
    {
        AgentClient = agentClient;
        LocalWorkFileManager = localWorkFileManager;
    }

    public IDatabaseApiClient AgentClient { get; }
    public FileManager LocalWorkFileManager { get; } //ლოკალური ფოლდერის მენეჯერი

    public static MultiDatabaseProcessStepParameters? Create(ILogger logger, IHttpClientFactory httpClientFactory,
        bool useConsole, string? webAgentName, ApiClients apiClients, string? databaseServerConnectionName,
        DatabaseServerConnections databaseServerConnections, string procLogFilesFolder)
    {
        var agentClient = DatabaseAgentClientsFabric.CreateDatabaseManagementClient(useConsole, logger,
            httpClientFactory, webAgentName, apiClients, databaseServerConnectionName, databaseServerConnections, null,
            null, CancellationToken.None).Result;

        if (agentClient is null)
        {
            StShared.WriteErrorLine($"DatabaseManagementClient does not created for webAgent {webAgentName}",
                useConsole, logger);
            return null;
        }

        var localWorkFileManager =
            FileManagersFabric.CreateFileManager(useConsole, logger, procLogFilesFolder);

        if (localWorkFileManager != null)
            return new MultiDatabaseProcessStepParameters(agentClient, localWorkFileManager);

        logger.LogError("workFileManager for procLogFilesFolder does not created");
        return null;
    }
}