﻿using System.Linq;
using ConnectTools;
using FileManagersMain;
using LibFileParameters.Models;

// ReSharper disable ConvertToPrimaryConstructor

namespace LibApAgentData.FolderProcessors;

public sealed class DeleteRedundantFiles : FolderProcessor
{
    private readonly FileManager _sourceFileManager;

    public DeleteRedundantFiles(FileManager sourceFileManager, FileManager destinationFileManager,
        ExcludeSet excludeSet) : base("Delete redundant files", "Delete redundant files after compare two places",
        destinationFileManager, null, true, excludeSet, true, true)
    {
        _sourceFileManager = sourceFileManager;
    }

    //success, folderNameChanged, continueWithThisFolder
    protected override (bool, bool, bool) ProcessOneFolder(string? afterRootPath, string folderName)
    {
        //დავადგინოთ ასეთი ფოლდერი გვაქვს თუ არა წყაროში და თუ არ გვაქვს წავშალოთ მიზნის მხარესაც

        var folders = _sourceFileManager.GetFolderNames(afterRootPath, null);

        if (folders.Contains(folderName))
            return (true, false, false);
        var deleted = FileManager.DeleteDirectory(afterRootPath, folderName, true);
        return deleted ? (true, true, true) : (false, false, false);
    }

    protected override bool ProcessOneFile(string? afterRootPath, MyFileInfo file)
    {
        var myFileInfos = _sourceFileManager.GetFilesWithInfo(afterRootPath, null);

        if ((ExcludeSet != null && ExcludeSet.NeedExclude(FileManager.PathCombine(afterRootPath, file.FileName))) ||
            !myFileInfos.Select(x => x.FileName).Contains(file.FileName))
            return FileManager.DeleteFile(afterRootPath, file.FileName);

        return true;
    }
}