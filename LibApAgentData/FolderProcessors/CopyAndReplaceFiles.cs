﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ConnectTools;
using FileManagersMain;
using LibApAgentData.StepCommands;
using LibFileParameters.Models;
using Microsoft.Extensions.Logging;

namespace LibApAgentData.FolderProcessors;

public sealed class CopyAndReplaceFiles : FolderProcessor
{
    private readonly Dictionary<string, List<MyFileInfo>> _checkedFolderFiles = new();


    private readonly List<string> _checkedFolders = new();
    private readonly FileManager _destinationFileManager;
    private readonly int _fileMaxLength;
    private readonly ILogger _logger;
    private readonly string _tempExtension;
    private readonly EMoveMethod _useMethod;

    public CopyAndReplaceFiles(ILogger logger, FileManager sourceFileManager, FileManager destinationFileManager,
        EMoveMethod useMethod, string uploadTempExtension, string downloadTempExtension, ExcludeSet excludeSet,
        int destinationFileMaxLength) : base("Copy And Replace files",
        "Copy And Replace files from one place to another", sourceFileManager, null, true, excludeSet)
    {
        _destinationFileManager = destinationFileManager;
        _logger = logger;
        _useMethod = useMethod;
        _tempExtension = _useMethod switch
        {
            EMoveMethod.Upload => uploadTempExtension,
            EMoveMethod.Download => downloadTempExtension,
            EMoveMethod.Local => "",
            _ => throw new ArgumentOutOfRangeException()
        };
        _fileMaxLength = destinationFileMaxLength - _tempExtension.Length;
    }

    protected override bool ProcessOneFile(string? afterRootPath, MyFileInfo file)
    {
        var dirNames = afterRootPath is null
            ? new List<string>()
            : afterRootPath.PrepareAfterRootPath(FileManager.DirectorySeparatorChar);
        var preparedDestinationAfterRootPath = CheckDestinationDirs(dirNames);


        var preparedFileName = file.FileName.PreparedFileNameConsideringLength(_fileMaxLength);

        var myFileInfo = GetOneFileWithInfo(preparedDestinationAfterRootPath, preparedFileName);

        //თუ ფაილის სახელი და სიგრძე ემთხვევა, ვთვლით, რომ იგივე ფაილია
        if (myFileInfo != null && myFileInfo.FileLength == file.FileLength)
            return true;

        if (myFileInfo != null)
            //იგივე სახელით ფაილი არსებობს და ამიტომ ჯერ უნდა წაიშალოს
            _destinationFileManager.DeleteFile(preparedDestinationAfterRootPath, preparedFileName);

        switch (_useMethod)
        {
            case EMoveMethod.Upload:
                if (!_destinationFileManager.UploadFile(afterRootPath, file.FileName,
                        preparedDestinationAfterRootPath,
                        preparedFileName, _tempExtension))
                {
                    //თუ ვერ აიტვირთა, გადავდივართ შემდეგზე
                    _logger.LogWarning($"Folder with name {file.FileName} cannot Upload");
                    return true;
                }

                break;
            case EMoveMethod.Download:
                if (!FileManager.DownloadFile(afterRootPath, file.FileName, preparedDestinationAfterRootPath,
                        preparedFileName,
                        _tempExtension))
                {
                    //თუ ვერ აიტვირთა, გადავდივართ შემდეგზე
                    _logger.LogWarning($"File with name {file.FileName} cannot Download");
                    return true;
                }

                break;
            case EMoveMethod.Local:
                File.Copy(FileManager.GetPath(afterRootPath, file.FileName),
                    _destinationFileManager.GetPath(preparedDestinationAfterRootPath, preparedFileName));
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return true;
    }

    private MyFileInfo? GetOneFileWithInfo(string? afterRootPath, string fileName)
    {
        return GetFileInfos(afterRootPath).SingleOrDefault(x => x.FileName == fileName);
    }

    private List<MyFileInfo> GetFileInfos(string? afterRootPath)
    {
        if (afterRootPath is null)
            return _destinationFileManager.GetFilesWithInfo(null, null);
        if (!_checkedFolderFiles.ContainsKey(afterRootPath))
            _checkedFolderFiles.Add(afterRootPath, _destinationFileManager.GetFilesWithInfo(afterRootPath, null));
        return _checkedFolderFiles[afterRootPath];
    }

    private string? CheckDestinationDirs(IEnumerable<string> dirNames)
    {
        string? afterRootPath = null;
        foreach (var dir in dirNames)
        {
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