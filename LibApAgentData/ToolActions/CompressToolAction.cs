using System.IO;
using LibApAgentData.Domain;
using LibFileParameters.Models;
using LibToolActions.BackgroundTasks;
using Microsoft.Extensions.Logging;
using SystemToolsShared;
using WebAgentProjectsApiContracts.V1.Responses;

namespace LibApAgentData.ToolActions;

public sealed class CompressToolAction : ProcessesToolAction
{
    private readonly BackupFileParameters _backupFileParameters;
    private readonly SmartSchema _localSmartSchema;
    private readonly bool _useConsole;
    private readonly CompressParameters? _par;
    private readonly FileStorageData _uploadFileStorage;
    private readonly UploadParameters _uploadParameters;


    public CompressToolAction(ILogger logger, bool useConsole, ProcessManager? processManager, CompressParameters? par,
        UploadParameters uploadParameters, BackupFileParameters backupFileParameters, int compressProcLine,
        SmartSchema localSmartSchema, FileStorageData uploadFileStorage) : base(logger, null, null, processManager,
        "Compress Backup", compressProcLine)
    {
        _useConsole = useConsole;
        _par = par;
        _uploadParameters = uploadParameters;
        _backupFileParameters = backupFileParameters;
        _localSmartSchema = localSmartSchema;
        _uploadFileStorage = uploadFileStorage;
    }

    public override ProcessesToolAction? GetNextAction()
    {
        var uploadToolAction = new UploadToolAction(Logger, ProcessManager, _uploadParameters, _backupFileParameters);

        return NeedUpload(_uploadFileStorage) ? uploadToolAction : uploadToolAction.GetNextAction();
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
        return FileStat.NormalizePath(_uploadParameters.LocalPath) !=
               FileStat.NormalizePath(uploadFileStorage.FileStoragePath);
    }

    protected override bool RunAction()
    {
        if (_par is null)
            return true;

        var filesForCompress = _par.WorkFileManager.GetFilesByMask(_backupFileParameters.Prefix,
            _backupFileParameters.DateMask, _backupFileParameters.Suffix);

        foreach (var fileInfo in filesForCompress)
        {
            var sourceFileName = Path.Combine(_par.WorkPath, fileInfo.FileName);
            var destinationFileFullName = sourceFileName + _par.Archiver.FileExtension;
            var tempFileName = destinationFileFullName + _par.ArchiveTempExtension;
            if (!_par.Archiver.PathToArchive(sourceFileName, tempFileName))
            {
                File.Delete(tempFileName);
                return false;
            }

            File.Delete(destinationFileFullName);
            File.Move(tempFileName, destinationFileFullName);
            File.Delete(sourceFileName);
        }

        //დაგროვილი ზედმეტი ფაილების წაშლა
        _par.WorkFileManager.RemoveRedundantFiles(_backupFileParameters.Prefix, _backupFileParameters.DateMask,
            _backupFileParameters.Suffix + _par.Archiver.FileExtension, _localSmartSchema);

        //გავასწოროთ სუფიქსი, რადგან ფაილები დაარქივდა
        _backupFileParameters.Suffix += _par.Archiver.FileExtension;


        return true;
    }
}