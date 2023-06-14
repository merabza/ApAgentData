using System;
using System.IO;
using System.Linq;
using DbTools;
using DbTools.Models;
using LibApAgentData.Domain;
using LibApAgentData.Models;
using LibApAgentData.Steps;
using LibApAgentData.ToolActions;
using LibToolActions.BackgroundTasks;
using Microsoft.Extensions.Logging;
using SystemToolsShared;

namespace LibApAgentData.StepCommands;

public sealed class DatabaseBackupStepCommand : ProcessesToolAction
{
    private readonly string _downloadTempExtension;

    private readonly JobStep _jobStep;
    private readonly DatabaseBackupStepParameters _par;

    public DatabaseBackupStepCommand(ILogger logger, bool useConsole, ProcessManager processManager, JobStep jobStep,
        DatabaseBackupStepParameters par, string downloadTempExtension) : base(logger, useConsole, processManager,
        "Database Backup", jobStep.ProcLineId)
    {
        _jobStep = jobStep;
        _par = par;
        _downloadTempExtension = downloadTempExtension;
    }

    protected override bool RunAction()
    {
        Logger.LogInformation("Checking parameters...");

        //1. თუ ლოკალური ფოლდერი არ არსებობს, შეიქმნას
        if (!Directory.Exists(_par.LocalPath))
        {
            Logger.LogInformation($"Creating local folder {_par.LocalPath}");
            Directory.CreateDirectory(_par.LocalPath);
        }

        //დადგინდეს არსებული ბაზების სია
        var databaseInfos = _par.AgentClient.GetDatabaseNames().Result;

        var dbInfos = databaseInfos.Where(w =>
            w.RecoveryModel != EDatabaseRecovery.Simple || _par.BackupType != EBackupType.TrLog);

        //დადგინდეს დასაბექაპებელი ბაზების სია
        var databaseNames = _par.DatabaseSet switch
        {
            EDatabaseSet.AllDatabases => dbInfos.Select(s => s.Name).ToList(),
            EDatabaseSet.SystemDatabases => dbInfos.Where(w => w.IsSystemDatabase).Select(s => s.Name).ToList(),
            EDatabaseSet.AllUserDatabases => dbInfos.Where(w => !w.IsSystemDatabase).Select(s => s.Name).ToList(),
            EDatabaseSet.DatabasesBySelection => dbInfos.Select(s => s.Name)
                .Intersect(_par.DatabaseNames)
                .ToList(),
            _ => throw new ArgumentOutOfRangeException()
        };

        //თუ ბაზების არჩევა ხდება სიიდან, მაშინ უნდა შევამოწმოთ ხომ არ არის სიაში ისეთი ბაზა, რომელიც სერვერზე არ არის.
        //თუ ასეთი აღმოჩნდა, გამოვიტანოთ ინფორმაცია ამის შესახებ
        if (_par.DatabaseSet == EDatabaseSet.DatabasesBySelection)
        {
            var missingDatabaseNames = _par.DatabaseNames.Except(databaseInfos.Select(s => s.Name)).ToList();

            if (missingDatabaseNames.Count > 0)
                foreach (var databaseName in missingDatabaseNames)
                    Logger.LogWarning($"Database with name {databaseName} is missing");
        }

        var needDownload = NeedDownload();
        //თითოეული ბაზისათვის გაკეთდეს ბაქაპირების პროცესი
        foreach (var databaseName in databaseNames)
        {
            if (ProcessManager is not null && ProcessManager.CheckCancellation())
                return false;
            var backupFileNamePrefix = _par.DbBackupParameters.GetPrefix(databaseName);

            var backupFileNameSuffix = _par.DbBackupParameters.GetSuffix() + (_par.CompressParameters is null
                ? ""
                : _par.CompressParameters.Archiver.FileExtension.AddNeedLeadPart("."));

            //შემოწმდეს ამ პერიოდში უკვე ხომ არ არის გაკეთებული ამ ბაზის ბექაპი
            if (HaveCurrentPeriodFile(backupFileNamePrefix, _par.DbBackupParameters.DateMask, backupFileNameSuffix))
                continue;

            Logger.LogInformation($"Backup database {databaseName}...");

            var backupFileParameters =
                _par.AgentClient.CreateBackup(_par.DbBackupParameters, databaseName).Result;

            //თუ ბექაპის დამზადებისას რაიმე პრობლემა დაფისირდა, ვჩერდებით.
            if (backupFileParameters == null)
            {
                Logger.LogError($"Backup for database {databaseName} not created");
                continue;
            }

            _par.DownloadFileManager.RemoveRedundantFiles(backupFileParameters.Prefix, backupFileParameters.DateMask,
                backupFileParameters.Suffix, _par.DownloadSideSmartSchema);

            var downloadBackupParameters =
                DownloadBackupParameters.Create(Logger, UseConsole, _par.LocalPath, _par.DownloadFileStorageData);

            if (downloadBackupParameters is null)
            {
                StShared.WriteErrorLine("downloadBackupParameters does not created", UseConsole, Logger);
                return false;
            }

            //მოქაჩვის პროცესის გაშვება
            var downloadBackupToolAction = new DownloadBackupToolAction(Logger, UseConsole,
                ProcessManager, downloadBackupParameters, _par.DownloadProcLineId, backupFileParameters,
                _downloadTempExtension, _par.CompressProcLineId, _par.LocalSmartSchema, _par.UploadFileStorageData,
                _par.CompressParameters, _par.UploadParameters);
            if (ProcessManager is not null && ProcessManager.CheckCancellation())
                return false;

            //აქ შემდეგი მოქმედების გამოძახება ხდება, იმიტომ რომ თითოეული ბაზისათვის ცალკე მოქმედებების ჯაჭვის აგება ხდება
            var nextAction =
                needDownload ? downloadBackupToolAction : downloadBackupToolAction.GetNextAction();
            RunNextAction(nextAction);
        }

        return true;
    }


    private bool HaveCurrentPeriodFile(string processName, string dateMask, string extension)
    {
        var currentPeriodFileChecker = new CurrentPeriodFileChecker(_jobStep.PeriodType,
            _jobStep.StartAt, _jobStep.HoleStartTime, _jobStep.HoleEndTime, processName, dateMask, extension,
            _par.LocalWorkFileManager);
        return currentPeriodFileChecker.HaveCurrentPeriodFile();
    }


    private bool NeedDownload()
    {
        var fileStoragePath = _par.DownloadFileStorageData.FileStoragePath;

        if (string.IsNullOrWhiteSpace(fileStoragePath))
            return false;

        //თუ ბაზის ფაილსაცავი ქსელურია, მოქაჩვა გვჭირდება
        if (!FileStat.IsFileSchema(fileStoragePath))
            return true;

        //თუ ბაზის ფაილსაცავი ლოკალურია და მისი ფოლდერი ემთხვევა ლოკალურ ფოლდერს
        //მაშინ მოქაჩვა არ გვჭირდება
        //თუ ფოლდერები არ ემთხვევა მოქაჩვა გვჭირდება
        return FileStat.NormalizePath(_par.LocalPath) != FileStat.NormalizePath(fileStoragePath);
    }
}