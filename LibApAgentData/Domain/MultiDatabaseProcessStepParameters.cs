using System.Net.Http;
using System.Threading;
using DatabasesManagement;
using FileManagersMain;
using LibApiClientParameters;
using LibDatabaseParameters;
using Microsoft.Extensions.Logging;
using SystemToolsShared.Errors;

namespace LibApAgentData.Domain;

public sealed class MultiDatabaseProcessStepParameters
{
    // ReSharper disable once ConvertToPrimaryConstructor
    private MultiDatabaseProcessStepParameters(IDatabaseManager agentClient, FileManager localWorkFileManager)
    {
        AgentClient = agentClient;
        LocalWorkFileManager = localWorkFileManager;
    }

    public IDatabaseManager AgentClient { get; }
    public FileManager LocalWorkFileManager { get; } //ლოკალური ფოლდერის მენეჯერი

    public static MultiDatabaseProcessStepParameters? Create(ILogger logger, IHttpClientFactory httpClientFactory,
        bool useConsole, string? webAgentName, ApiClients apiClients, string? databaseServerConnectionName,
        DatabaseServerConnections databaseServerConnections, string procLogFilesFolder)
    {
        var createDatabaseManagerResult = DatabaseManagersFabric.CreateDatabaseManager(logger, useConsole,
            databaseServerConnectionName, databaseServerConnections, apiClients, httpClientFactory, null, null,
            CancellationToken.None).Preserve().Result;

        if (createDatabaseManagerResult.IsT1) Err.PrintErrorsOnConsole(createDatabaseManagerResult.AsT1);

        var localWorkFileManager = FileManagersFabric.CreateFileManager(useConsole, logger, procLogFilesFolder);

        if (localWorkFileManager != null)
            return new MultiDatabaseProcessStepParameters(createDatabaseManagerResult.AsT0, localWorkFileManager);

        logger.LogError("workFileManager for procLogFilesFolder does not created");
        return null;
    }
}