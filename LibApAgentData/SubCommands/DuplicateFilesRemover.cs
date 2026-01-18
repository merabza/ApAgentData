using System;
using System.Collections.Generic;
using ApAgentData.LibApAgentData.Models;
using SystemToolsShared;

namespace ApAgentData.LibApAgentData.SubCommands;

public sealed class DuplicateFilesRemover
{
    private readonly FileListModel _fileList;

    private readonly List<string> _priorityList;
    private readonly bool _useConsole;

    // ReSharper disable once ConvertToPrimaryConstructor
    public DuplicateFilesRemover(bool useConsole, FileListModel fileList, List<string> priorityList)
    {
        _useConsole = useConsole;
        _fileList = fileList;
        _priorityList = priorityList;
    }

    internal bool Run()
    {
        Console.WriteLine("Delete duplicate Files");

        StShared.ConsoleWriteInformationLine(null, _useConsole, "Remove Duplicate Files");
        foreach (var kvp in _fileList.DuplicateFilesStorage)
        {
            kvp.Value.RemoveDuplicates(_priorityList);
        }

        StShared.ConsoleWriteInformationLine(null, _useConsole, "Finish");

        return true;
    }
}
