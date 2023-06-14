using System;
using ConnectTools;
using FileManagersMain;

namespace LibApAgentData.FolderProcessors;

public sealed class DeleteTempFiles : FolderProcessor
{
    private readonly string[] _patterns;

    public DeleteTempFiles(FileManager destinationFileManager, string[] patterns) : base("Temp files",
        "Delete Temp files", destinationFileManager, null, true, null, null)
    {
        _patterns = patterns;
    }

    protected override bool CheckParameters()
    {
        if (_patterns is { Length: > 0 })
            return true;
        Console.WriteLine("Delete Files patterns not specified");
        return false;
    }

    protected override bool ProcessOneFile(string? destinationAfterRootPath, MyFileInfo file,
        RecursiveParameters? recursiveParameters = null)
    {
        return !NeedExclude(file.FileName, _patterns) ||
               FileManager.DeleteFile(destinationAfterRootPath, file.FileName);
    }
}