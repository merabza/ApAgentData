﻿using LibApAgentData.Domain;
using LibFileParameters.Models;
using LibToolActions.BackgroundTasks;
using Microsoft.Extensions.Logging;
using WebAgentContracts.V1.Responses;

namespace LibApAgentData.ToolActions;

public sealed class DownloadBackupToolAction : ProcessesToolAction
{
    private readonly BackupFileParameters _backupFileParameters;
    private readonly CompressParameters? _compressParameters;
    private readonly int _compressProcLine;
    private readonly string _downloadTempExtension;
    private readonly SmartSchema _localSmartSchema;
    private readonly DownloadBackupParameters _par;
    private readonly FileStorageData _uploadFileStorage;
    private readonly UploadParameters _uploadParameters;


    public DownloadBackupToolAction(ILogger logger, bool useConsole, ProcessManager? processManager,
        DownloadBackupParameters downloadBackupParameters, int downloadProcLineId,
        BackupFileParameters backupFileParameters, string downloadTempExtension, int compressProcLine,
        SmartSchema localSmartSchema, FileStorageData uploadFileStorage, CompressParameters? compressParameters,
        UploadParameters uploadParameters) : base(logger, useConsole, processManager, "Download Backup",
        downloadProcLineId)
    {
        _par = downloadBackupParameters;
        _backupFileParameters = backupFileParameters;
        _downloadTempExtension = downloadTempExtension;
        _compressProcLine = compressProcLine;
        _localSmartSchema = localSmartSchema;
        _uploadFileStorage = uploadFileStorage;
        _compressParameters = compressParameters;
        _uploadParameters = uploadParameters;
    }

    public override ProcessesToolAction? GetNextAction()
    {
        var compressToolAction = new CompressToolAction(Logger, UseConsole, ProcessManager, _compressParameters,
            _uploadParameters, _backupFileParameters, _compressProcLine, _localSmartSchema, _uploadFileStorage);

        if (_compressParameters is not null) return compressToolAction;
        _par.LocalFileManager.RemoveRedundantFiles(_backupFileParameters.Prefix, _backupFileParameters.DateMask,
            _backupFileParameters.Suffix, _localSmartSchema);
        return compressToolAction.GetNextAction();
    }

    protected override bool RunAction()
    {
        var success = _par.DownloadFileManager.DownloadFile(_backupFileParameters.Name, _downloadTempExtension);

        _par.LocalFileManager.RemoveRedundantFiles(_backupFileParameters.Prefix, _backupFileParameters.DateMask,
            _backupFileParameters.Suffix, _localSmartSchema);

        return success;
    }
}