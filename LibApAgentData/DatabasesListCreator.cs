using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DatabasesManagement;
using DbTools;
using DbTools.Models;
using LibApAgentData.Models;

namespace LibApAgentData;

public sealed class DatabasesListCreator
{
    //private readonly bool _byParameters;
    private readonly IDatabaseManager _agentClient;

    private readonly EBackupType? _backupType;
    private readonly EDatabaseSet _databaseSet;


    // ReSharper disable once ConvertToPrimaryConstructor
    public DatabasesListCreator(EDatabaseSet databaseSet, IDatabaseManager agentClient,
        EBackupType? backupType = null)
    {
        _databaseSet = databaseSet;
        _agentClient = agentClient;
        _backupType = backupType;
    }

    public async Task<List<DatabaseInfoModel>> LoadDatabaseNames(CancellationToken cancellationToken)
    {
        var getDatabaseNamesResult = await _agentClient.GetDatabaseNames(cancellationToken);
        var databaseInfos = getDatabaseNamesResult.AsT0;

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
            case EDatabaseSet.AllDatabases:
                break;
            case EDatabaseSet.DatabasesBySelection:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return databaseInfos.Where(w =>
            (!sysBaseDoesMatter || w.IsSystemDatabase == checkSysBase) &&
            (w.RecoveryModel != EDatabaseRecovery.Simple || _backupType != EBackupType.TrLog)).ToList();
    }
}