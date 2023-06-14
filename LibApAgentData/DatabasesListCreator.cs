using System.Collections.Generic;
using System.Linq;
using DatabaseManagementClients;
using DbTools;
using DbTools.Models;
using LibApAgentData.Models;

namespace LibApAgentData;

public sealed class DatabasesListCreator
{
    //private readonly bool _byParameters;
    private readonly DatabaseManagementClient _agentClient;

    private readonly EBackupType? _backupType;
    private readonly EDatabaseSet _databaseSet;


    public DatabasesListCreator(EDatabaseSet databaseSet, DatabaseManagementClient agentClient,
        EBackupType? backupType = null)
    {
        _databaseSet = databaseSet;
        _agentClient = agentClient;
        _backupType = backupType;
    }

    public List<DatabaseInfoModel> LoadDatabaseNames()
    {
        var databaseInfos = _agentClient.GetDatabaseNames().Result;

        var sysBaseDoesMatter = false;
        var checkSysBase = false;
        switch (_databaseSet)
        {
            case EDatabaseSet.SystemDatabases:
                checkSysBase = true;
                sysBaseDoesMatter = true;
                break;
            case EDatabaseSet.AllUserDatabases:
                sysBaseDoesMatter = true;
                break;
        }

        return databaseInfos.Where(w => (!sysBaseDoesMatter || w.IsSystemDatabase == checkSysBase) &&
                                        (w.RecoveryModel != EDatabaseRecovery.Simple ||
                                         _backupType != EBackupType.TrLog)).ToList();
    }
}