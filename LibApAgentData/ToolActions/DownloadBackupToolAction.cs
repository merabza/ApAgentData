using System.Threading;
using System.Threading.Tasks;
using ApAgentData.LibApAgentData.Domain;
using LibFileParameters.Models;
using LibToolActions.BackgroundTasks;
using Microsoft.Extensions.Logging;
using WebAgentDatabasesApiContracts.V1.Responses;

namespace ApAgentData.LibApAgentData.ToolActions;

public sealed class DownloadBackupToolAction : ProcessesToolAction
{
    private readonly BackupFileParameters _backupFileParameters;
    private readonly CompressParameters? _compressParameters;
    private readonly int _compressProcLine;
    private readonly string _downloadTempExtension;
    private readonly SmartSchema _localSmartSchema;
    private readonly ILogger _logger;
    private readonly DownloadBackupParameters _par;
    private readonly FileStorageData _uploadFileStorage;
    private readonly UploadParameters _uploadParameters;
    private readonly bool _useConsole;

    // ReSharper disable once ConvertToPrimaryConstructor
    public DownloadBackupToolAction(ILogger logger, bool useConsole, ProcessManager? processManager,
        DownloadBackupParameters downloadBackupParameters, int downloadProcLineId,
        BackupFileParameters backupFileParameters, string downloadTempExtension, int compressProcLine,
        SmartSchema localSmartSchema, FileStorageData uploadFileStorage, CompressParameters? compressParameters,
        UploadParameters uploadParameters) : base(logger, null, null, processManager, "Download Backup",
        downloadProcLineId)
    {
        _logger = logger;
        _useConsole = useConsole;
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
        var compressToolAction = new CompressToolAction(_logger, _useConsole, ProcessManager, _compressParameters,
            _uploadParameters, _backupFileParameters, _compressProcLine, _localSmartSchema, _uploadFileStorage);

        if (_compressParameters is not null)
        {
            return compressToolAction;
        }

        _par.LocalFileManager.RemoveRedundantFiles(_backupFileParameters.Prefix, _backupFileParameters.DateMask,
            _backupFileParameters.Suffix, _localSmartSchema);
        return compressToolAction.GetNextAction();
    }

    protected override ValueTask<bool> RunAction(CancellationToken cancellationToken = default)
    {
        var success = _par.DownloadFileManager.DownloadFile(_backupFileParameters.Name, _downloadTempExtension);

        _par.LocalFileManager.RemoveRedundantFiles(_backupFileParameters.Prefix, _backupFileParameters.DateMask,
            _backupFileParameters.Suffix, _localSmartSchema);

        return ValueTask.FromResult(success);
    }
}
