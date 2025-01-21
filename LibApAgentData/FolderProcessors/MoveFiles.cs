using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ConnectTools;
using FileManagersMain;
using LibApAgentData.StepCommands;
using LibFileParameters.Models;
using Microsoft.Extensions.Logging;

namespace LibApAgentData.FolderProcessors;

public sealed class MoveFiles : FolderProcessor
{
    private readonly List<string> _checkedFolders = [];
    private readonly FileManager _destinationFileManager;
    private readonly int _fileMaxLength;
    private readonly ILogger _logger;
    private readonly int _maxFolderCount;
    private readonly string? _moveFolderName;
    private readonly string _tempExtension;
    private readonly EMoveMethod _useMethod;

    public MoveFiles(ILogger logger, FileManager sourceFileManager, FileManager destinationFileManager,
        string? moveFolderName, EMoveMethod useMethod, string uploadTempExtension, string downloadTempExtension,
        ExcludeSet excludeSet, int maxFolderCount, int destinationFileMaxLength) : base("Move files",
        "Move files from one place to another", sourceFileManager, null, true, excludeSet, true, true)
    {
        _destinationFileManager = destinationFileManager;
        _moveFolderName = moveFolderName?.Trim();
        _logger = logger;
        _useMethod = useMethod;
        _maxFolderCount = maxFolderCount;
        _tempExtension = _useMethod switch
        {
            EMoveMethod.Upload => uploadTempExtension,
            EMoveMethod.Download => downloadTempExtension,
            EMoveMethod.Local => string.Empty,
            _ => throw new ArgumentOutOfRangeException()
        };
        _fileMaxLength = destinationFileMaxLength - _tempExtension.Length;
    }

    protected override bool ProcessOneFile(string? afterRootPath, MyFileInfo file)
    {
        var dirNames = afterRootPath == null
            ? new List<string>()
            : afterRootPath.Split(FileManager.DirectorySeparatorChar).TakeLast(_maxFolderCount).Select(s => s.Trim())
                .ToList();
        var destinationAfterRootPath = CheckDestinationDirs(dirNames);


        var preparedFileName = file.FileName.PrepareFileName().Trim();
        var extension = Path.GetExtension(preparedFileName).Trim();
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(preparedFileName).Trim();

        var i = 0;
        while (_destinationFileManager.FileExists(destinationAfterRootPath,
                   fileNameWithoutExtension.GetNewFileNameWithMaxLength(i, extension, _fileMaxLength)))
            i++;

        preparedFileName = fileNameWithoutExtension.GetNewFileNameWithMaxLength(i, extension, _fileMaxLength);

        switch (_useMethod)
        {
            case EMoveMethod.Upload:
                if (!_destinationFileManager.UploadFile(afterRootPath, file.FileName, destinationAfterRootPath,
                        preparedFileName, _tempExtension))
                {
                    //თუ ვერ აიტვირთა, გადავდივართ შემდეგზე
                    _logger.LogWarning($"Folder with name {file.FileName} cannot Upload");
                    return true;
                }

                FileManager.DeleteFile(afterRootPath, file.FileName);
                break;
            case EMoveMethod.Download:
                if (!FileManager.DownloadFile(afterRootPath, file.FileName, destinationAfterRootPath, preparedFileName,
                        _tempExtension))
                {
                    //თუ ვერ აიტვირთა, გადავდივართ შემდეგზე
                    _logger.LogWarning($"File with name {file.FileName} cannot Download");
                    return true;
                }

                FileManager.DeleteFile(afterRootPath, file.FileName);
                break;
            case EMoveMethod.Local:
                File.Move(FileManager.GetPath(afterRootPath, file.FileName),
                    _destinationFileManager.GetPath(destinationAfterRootPath, preparedFileName));
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return true;
    }

    private string? CheckDestinationDirs(IEnumerable<string> dirNames)
    {
        var afterRootPath = _moveFolderName;
        foreach (var dir in dirNames)
        {
            var validDir = dir;
            //როცა ფოლდერის სახელის ბოლოში არის წერტილი, ეს ცუდად მოქმედებს შემდეგ პროცესებზე
            //FTP-ს მხარეს გადაწერა ხერხდება, მაგრამ FTP-დან ლინუქსზე ვეღარ.
            //ვინდოუსი ჭკვიანურად იქცევა და ასეთი ფოლდერის შექმნისას თვითონ აჭრის ბოლო წერტილებს.
            //თუმცა ეს პროგრამა ლინუქსზეც ეშვება. ამიტომ აქ ვითვალისწინებ ამ პრობლემას.
            //თუ უწერტილო ფოლდერის სახელს დაემთხვა პრობლემა არაა
            while (validDir.EndsWith('.'))
                validDir = validDir.TrimEnd('.');
            if (validDir.Length == 0) //თუ ფოლდერის სახელი მხოლოდ წერტილებისაგან შედგებოდა, ასეთი ფოლდერი ვერ შეიქმნება
                return null;
            var forCreateDirPart = _destinationFileManager.PathCombine(afterRootPath, dir);
            if (!_checkedFolders.Contains(forCreateDirPart))
            {
                if (!_destinationFileManager.CareCreateDirectory(afterRootPath, dir, true))
                    return null;
                _checkedFolders.Add(forCreateDirPart);
            }

            afterRootPath = forCreateDirPart;
        }

        return afterRootPath;
    }
}