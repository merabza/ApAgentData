//using CliParametersApiClientsEdit.Models;
//using CliParametersDataEdit.Models;
//using LibApAgentData.Domain;
//using LibToolActions.BackgroundTasks;
//using Microsoft.Extensions.Logging;

//namespace LibApAgentData.ToolActions;

//public sealed class DatabaseProcessesToolAction : ProcessesToolAction
//{
//    protected readonly DatabaseProcessesParameters Par;
//    //protected readonly string DatabaseServerConnectionName;
//    //protected readonly DatabaseServerConnections DatabaseServerConnections;
//    //protected readonly string WebAgentName;
//    //protected readonly ApiClients ApiClients;
//    //protected readonly DatabaseManagementClient DatabaseManagementClient;

//    //, DatabaseManagementClient databaseManagementClient
//    protected DatabaseProcessesToolAction(ILogger logger, bool useConsole, ProcessManager processManager,
//        string actionName, int procLineId, DatabaseProcessesParameters par, string databaseServerConnectionName,
//        DatabaseServerConnections databaseServerConnections, string webAgentName, ApiClients apiClients) : base(logger,
//        useConsole, processManager, actionName, null, procLineId)
//    {
//        Par = par;
//        //DatabaseServerConnectionName = databaseServerConnectionName;
//        //DatabaseServerConnections = databaseServerConnections;
//        //WebAgentName = webAgentName;
//        //ApiClients = apiClients;
//        //DatabaseManagementClient = databaseManagementClient;
//    }

//    //protected DatabaseManagementClient? GetDatabaseConnectionSettings()
//    //{
//    //    DatabaseManagementClient? agentClient = DatabaseAgentClientsFabric.CreateDatabaseManagementClient(UseConsole,
//    //        Logger, WebAgentName, ApiClients, DatabaseServerConnectionName, DatabaseServerConnections);

//    //    return agentClient;
//    //}

//}

