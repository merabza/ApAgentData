﻿using System;
using System.IO;
using CompressionManagement;
using ConnectTools;
using FileManagersMain;
using Microsoft.Extensions.Logging;

// ReSharper disable ConvertToPrimaryConstructor

namespace LibApAgentData.FolderProcessors;

public sealed class UnZipOnPlace : FolderProcessor
{
    private readonly ILogger _logger;
    private readonly bool _useConsole;

    public UnZipOnPlace(ILogger logger, bool useConsole, FileManager fileManager) : base("Unzip",
        "UnZip Zip Files on Place", fileManager, "*.zip", false, null, true, true)
    {
        _logger = logger;
        _useConsole = useConsole;
    }

    protected override bool CheckParameters()
    {
        return FileManager is DiskFileManager;
    }

    protected override bool ProcessOneFile(string? afterRootPath, MyFileInfo file)
    {
        var zipFileName = Path.GetFileNameWithoutExtension(file.FileName);
        var i = 0;

        var zipFileFullName = FileManager.GetPath(afterRootPath, file.FileName);

        while (FileManager.DirectoryExists(afterRootPath, GetNewFolderName(zipFileName, i)))
            i++;

        var newFolderName = GetNewFolderName(zipFileName, i);
        FileManager.CreateDirectory(afterRootPath, newFolderName);
        var newDirFullName = FileManager.GetPath(afterRootPath, newFolderName);

        var archiver = new ZipClassArchiver(_logger, _useConsole, ".zip");

        Console.WriteLine($"Unzip {zipFileFullName}");

        if (!archiver.ArchiveToPath(zipFileFullName, newDirFullName))
            return false;

        FileManager.DeleteFile(afterRootPath, file.FileName);

        return true;
    }

    private static string GetNewFolderName(string zipFileName, int i)
    {
        return $"{zipFileName}{(i == 0 ? string.Empty : $"({i})")}";
    }
}