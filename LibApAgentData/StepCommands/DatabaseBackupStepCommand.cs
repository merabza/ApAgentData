using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApAgentData.LibApAgentData.Domain;
using ApAgentData.LibApAgentData.Models;
using ApAgentData.LibApAgentData.Steps;
using ApAgentData.LibApAgentData.ToolActions;
using DbTools;
using DbTools.Models;
using LibToolActions.BackgroundTasks;
using Microsoft.Extensions.Logging;
using SystemToolsShared;
using SystemToolsShared.Errors;

namespace ApAgentData.LibApAgentData.StepCommands;

public sealed class DatabaseBackupStepCommand : ProcessesToolAction
{
    private readonly string _downloadTempExtension;
    private readonly JobStep _jobStep;
    private readonly ILogger _logger;
    private readonly DatabaseBackupStepParameters _par;

    private readonly bool _useConsole;

    // ReSharper disable once ConvertToPrimaryConstructor
    public DatabaseBackupStepCommand(bool useConsole, ILogger logger, ProcessManager processManager, JobStep jobStep,
        DatabaseBackupStepParameters par, string downloadTempExtension) : base(logger, null, null, processManager,
        "Database Backup", jobStep.ProcLineId)
    {
        _useConsole = useConsole;
        _logger = logger;
        _jobStep = jobStep;
        _par = par;
        _downloadTempExtension = downloadTempExtension;
    }

    protected override async ValueTask<bool> RunAction(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Checking parameters...");

        var localPath = _par.LocalPath;

        //1. თუ ლოკალური ფოლდერი არ არსებობს, შეიქმნას
        if (!Directory.Exists(localPath))
        {
            _logger.LogInformation("Creating local folder {LocalPath}", localPath);
            Directory.CreateDirectory(localPath);
        }

        //დადგინდეს არსებული ბაზების სია
        var getDatabaseNamesResult = await _par.AgentClient.GetDatabaseNames(cancellationToken);
        if (getDatabaseNamesResult.IsT1)
        {
            Err.PrintErrorsOnConsole(getDatabaseNamesResult.AsT1);
            return false;
        }

        var databaseInfos = getDatabaseNamesResult.AsT0;

        var dbInfos = databaseInfos.Where(w =>
            w.RecoveryModel != EDatabaseRecoveryModel.Simple || _par.BackupType != EBackupType.TrLog);

        List<string> databaseNames = CountDatabaseNames(_par.DatabaseSet, dbInfos);

        //თუ ბაზების არჩევა ხდება სიიდან, მაშინ უნდა შევამოწმოთ ხომ არ არის სიაში ისეთი ბაზა, რომელიც სერვერზე არ არის.
        //თუ ასეთი აღმოჩნდა, გამოვიტანოთ ინფორმაცია ამის შესახებ
        if (_par.DatabaseSet == EDatabaseSet.DatabasesBySelection)
        {
            var missingDatabaseNames = _par.DatabaseNames.Except(databaseInfos.Select(s => s.Name)).ToList();

            if (missingDatabaseNames.Count > 0)
            {
                foreach (var databaseName in missingDatabaseNames)
                {
                    _logger.LogWarning("Database with name {DatabaseName} is missing", databaseName);
                }
            }
        }

        var needDownload = NeedDownload();
        //თითოეული ბაზისათვის გაკეთდეს ბაქაპირების პროცესი
        foreach (var databaseName in databaseNames)
        {
            if (ProcessManager is not null && ProcessManager.CheckCancellation())
            {
                return false;
            }

            var backupFileNamePrefix = _par.DbBackupParameters.GetPrefix(databaseName);

            var backupFileNameSuffix = _par.DbBackupParameters.GetSuffix() + (_par.CompressParameters is null
                ? string.Empty
                : _par.CompressParameters.Archiver.FileExtension.AddNeedLeadPart("."));

            //შემოწმდეს ამ პერიოდში უკვე ხომ არ არის გაკეთებული ამ ბაზის ბექაპი
            if (HaveCurrentPeriodFile(backupFileNamePrefix, _par.DbBackupParameters.DateMask, backupFileNameSuffix))
            {
                continue;
            }

            _logger.LogInformation("Backup database {DatabaseName}...", databaseName);

            var createBackupResult = await _par.AgentClient.CreateBackup(_par.DbBackupParameters, databaseName,
                _par.DbServerFoldersSetName, cancellationToken);
            if (createBackupResult.IsT1)
            {
                Err.PrintErrorsOnConsole(createBackupResult.AsT1);
                continue;
            }

            var backupFileParameters = createBackupResult.AsT0;

            //თუ ბექაპის დამზადებისას რაიმე პრობლემა დაფიქსირდა, ვჩერდებით.
            if (backupFileParameters == null)
            {
                _logger.LogError("Backup for database {DatabaseName} not created", databaseName);
                continue;
            }

            _par.DownloadFileManager.RemoveRedundantFiles(backupFileParameters.Prefix, backupFileParameters.DateMask,
                backupFileParameters.Suffix, _par.DownloadSideSmartSchema);

            var downloadBackupParameters =
                DownloadBackupParameters.Create(_logger, _useConsole, localPath, _par.DownloadFileStorageData);

            if (downloadBackupParameters is null)
            {
                StShared.WriteErrorLine("downloadBackupParameters does not created", _useConsole, _logger);
                return false;
            }

            //მოქაჩვის პროცესის გაშვება
            var downloadBackupToolAction = new DownloadBackupToolAction(_logger, _useConsole, ProcessManager,
                downloadBackupParameters, _par.DownloadProcLineId, backupFileParameters, _downloadTempExtension,
                _par.CompressProcLineId, _par.LocalSmartSchema, _par.UploadFileStorageData, _par.CompressParameters,
                _par.UploadParameters);
            if (ProcessManager is not null && ProcessManager.CheckCancellation())
            {
                return false;
            }

            //აქ შემდეგი მოქმედების გამოძახება ხდება, იმიტომ რომ თითოეული ბაზისათვის ცალკე მოქმედებების ჯაჭვის აგება ხდება
            var nextAction = needDownload ? downloadBackupToolAction : downloadBackupToolAction.GetNextAction();
            await RunNextAction(nextAction, cancellationToken);
        }

        return true;
    }

    private List<string> CountDatabaseNames(EDatabaseSet databaseSet, IEnumerable<DatabaseInfoModel> dbInfos)
    {
        //დადგინდეს დასაბექაპებელი ბაზების სია
        var databaseNames = databaseSet switch
        {
            EDatabaseSet.AllDatabases => dbInfos.Select(s => s.Name).ToList(),
            EDatabaseSet.SystemDatabases => dbInfos.Where(w => w.IsSystemDatabase).Select(s => s.Name).ToList(),
            EDatabaseSet.AllUserDatabases => dbInfos.Where(w => !w.IsSystemDatabase).Select(s => s.Name).ToList(),
            EDatabaseSet.DatabasesBySelection => dbInfos.Select(s => s.Name).Intersect(_par.DatabaseNames).ToList(),
            _ => throw new ArgumentOutOfRangeException(nameof(databaseSet), databaseSet, "Invalid database set value.")
        };
        return databaseNames;
    }

    private bool HaveCurrentPeriodFile(string processName, string dateMask, string extension)
    {
        var currentPeriodFileChecker = new CurrentPeriodFileChecker(_jobStep.PeriodType, _jobStep.StartAt,
            _jobStep.HoleStartTime, _jobStep.HoleEndTime, processName, dateMask, extension, _par.LocalWorkFileManager);
        return currentPeriodFileChecker.HaveCurrentPeriodFile();
    }

    private bool NeedDownload()
    {
        var fileStoragePath = _par.DownloadFileStorageData.FileStoragePath;

        if (string.IsNullOrWhiteSpace(fileStoragePath))
        {
            return false;
        }

        //თუ ბაზის ფაილსაცავი ქსელურია, მოქაჩვა გვჭირდება
        if (!FileStat.IsFileSchema(fileStoragePath))
        {
            return true;
        }

        //თუ ბაზის ფაილსაცავი ლოკალურია და მისი ფოლდერი ემთხვევა ლოკალურ ფოლდერს
        //მაშინ მოქაჩვა არ გვჭირდება
        //თუ ფოლდერები არ ემთხვევა მოქაჩვა გვჭირდება
        return FileStat.NormalizePath(_par.LocalPath) != FileStat.NormalizePath(fileStoragePath);
    }
}
