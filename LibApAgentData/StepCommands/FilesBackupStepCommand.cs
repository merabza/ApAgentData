using System;
using System.IO;
using System.Linq;
using CompressionManagement;
using LibApAgentData.Domain;
using LibApAgentData.Steps;
using LibApAgentData.ToolActions;
using LibFileParameters.Models;
using LibToolActions.BackgroundTasks;
using Microsoft.Extensions.Logging;
using SystemToolsShared;
using WebAgentProjectsApiContracts.V1.Responses;

namespace LibApAgentData.StepCommands;

public sealed class FilesBackupStepCommand : ProcessesToolAction
{
    private readonly JobStep _jobStep;
    private readonly bool _useConsole;
    private readonly FilesBackupStepParameters _par;

    public FilesBackupStepCommand(ILogger logger, bool useConsole, FilesBackupStepParameters par,
        ProcessManager processManager, JobStep jobStep) : base(logger, null, null, processManager, "Files Backup",
        jobStep.ProcLineId)
    {
        _useConsole = useConsole;
        _par = par;
        _jobStep = jobStep;
    }

    protected override bool RunAction()
    {
        Logger.LogInformation("Checking parameters...");

        var localPath = _par.LocalPath;

        //1. თუ ლოკალური ფოლდერი არ არსებობს, შეიქმნას
        if (!Directory.Exists(localPath))
        {
            Logger.LogInformation("Creating local folder {localPath}", localPath);
            Directory.CreateDirectory(localPath);
        }

        if (_par.BackupSeparately)
            return _par.BackupFolderPaths.All(kvpBackupFolderPath =>
                ExecuteBackup(_par.MaskName + kvpBackupFolderPath.Key, new[] { kvpBackupFolderPath.Value },
                    _par.Archiver, _par.ExcludeSet, _par.UploadFileStorage));

        return ExecuteBackup(_par.MaskName, _par.BackupFolderPaths.Select(s => s.Value).ToArray(), _par.Archiver,
            _par.ExcludeSet, _par.UploadFileStorage);
    }

    private bool ExecuteBackup(string maskName, string[] sources, Archiver archiver, ExcludeSet excludeSet,
        FileStorageData uploadFileStorage)
    {
        if (ProcessManager is not null && ProcessManager.CheckCancellation())
            return false;

        var backupFileNamePrefix = maskName;
        var backupFileNameSuffix = archiver.FileExtension.AddNeedLeadPart(".");

        //შემოწმდეს ამ პერიოდში უკვე ხომ არ არის გაკეთებული ამ ბაზის ბექაპი
        if (HaveCurrentPeriodFile(backupFileNamePrefix, _par.DateMask, backupFileNameSuffix))
            return true;

        var backupFileName = backupFileNamePrefix + DateTime.Now.ToString(_par.DateMask) +
                             backupFileNameSuffix;
        var backupFileFullName = Path.Combine(_par.LocalPath, backupFileName);

        var tempFileName = backupFileFullName + _par.ArchivingTempExtension.AddNeedLeadPart(".");

        if (!archiver.SourcesToArchive(sources, tempFileName, excludeSet.FolderFileMasks.ToArray()))
        {
            File.Delete(tempFileName);
            return false;
        }

        File.Move(tempFileName, backupFileFullName);

        //წაიშალოს ადრე შექმნილი დაძველებული ფაილები
        _par.LocalWorkFileManager.RemoveRedundantFiles(backupFileNamePrefix, _par.DateMask, backupFileNameSuffix,
            _par.LocalSmartSchema);

        var backupFileParameters = new BackupFileParameters(backupFileName, backupFileNamePrefix,
            backupFileNameSuffix, _par.DateMask);

        var uploadToolAction =
            new UploadToolAction(Logger, ProcessManager, _par.UploadParameters, backupFileParameters);

        var nextAction =
            NeedUpload(uploadFileStorage) ? uploadToolAction : uploadToolAction.GetNextAction();
        RunNextAction(nextAction);


        return true;
    }

    private bool HaveCurrentPeriodFile(string processName, string dateMask, string extension)
    {
        var currentPeriodFileChecker = new CurrentPeriodFileChecker(_jobStep.PeriodType,
            _jobStep.StartAt, _jobStep.HoleStartTime, _jobStep.HoleEndTime, processName,
            dateMask, extension, _par.LocalWorkFileManager);
        return currentPeriodFileChecker.HaveCurrentPeriodFile();
    }

    private bool NeedUpload(FileStorageData uploadFileStorage)
    {
        if (uploadFileStorage.FileStoragePath is null)
        {
            StShared.WriteWarningLine("uploadFileStorage.FileStoragePath does not specified", _useConsole, Logger);
            return false;
        }


        //თუ ასატვირთი ფაილსაცავი ქსელურია, აქაჩვა გვჭირდება
        if (!FileStat.IsFileSchema(uploadFileStorage.FileStoragePath))
            return true;

        //თუ ატვირთვის ფაილსაცავი ლოკალურია და მისი ფოლდერი ემთხვევა ლოკალურ ფოლდერს
        //მაშინ აქაჩვა არ გვჭირდება
        //თუ ფოლდერები არ ემთხვევა აქაჩვა გვჭირდება
        return FileStat.NormalizePath(_par.LocalPath) != FileStat.NormalizePath(uploadFileStorage.FileStoragePath);
    }
}