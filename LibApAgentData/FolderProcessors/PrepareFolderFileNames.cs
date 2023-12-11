using System;
using ConnectTools;
using FileManagersMain;
using LibApAgentData.StepCommands;
using LibFileParameters.Models;

namespace LibApAgentData.FolderProcessors;

public sealed class PrepareFolderFileNames : FolderProcessor
{
    private readonly int _fileMaxLength;

    public PrepareFolderFileNames(FileManager sourceFileManager, EMoveMethod useMethod, string uploadTempExtension,
        string downloadTempExtension, ExcludeSet excludeSet, int destinationFileMaxLength) : base("Prepare Names",
        "Prepare Folder and File Names", sourceFileManager, null, false, excludeSet)
    {
        var tempExtension = useMethod switch
        {
            EMoveMethod.Upload => uploadTempExtension,
            EMoveMethod.Download => downloadTempExtension,
            EMoveMethod.Local => "",
            _ => throw new ArgumentOutOfRangeException()
        };
        _fileMaxLength = destinationFileMaxLength - tempExtension.Length;
    }

    protected override (bool, bool) ProcessOneFolder(string? afterRootPath, string folderName)
    {
        var preparedFolderName = folderName.Trim();
        return preparedFolderName != folderName
            ? (FileManager.RenameFolder(afterRootPath, folderName, preparedFolderName), true)
            : (true, false);
    }

    protected override bool ProcessOneFile(string? afterRootPath, MyFileInfo file)
    {
        var preparedFileName = file.FileName.PreparedFileNameConsideringLength(_fileMaxLength);
        return preparedFileName == file.FileName ||
               FileManager.RenameFile(afterRootPath, file.FileName, preparedFileName);
    }
}