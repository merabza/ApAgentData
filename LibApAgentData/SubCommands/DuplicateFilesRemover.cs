using System;
using System.Collections.Generic;
using LibApAgentData.Models;
using SystemToolsShared;

namespace LibApAgentData.SubCommands;

public sealed class DuplicateFilesRemover
{
    private readonly FileListModel _fileList;

    private readonly List<string> _priorityList;
    private readonly bool _useConsole;

    public DuplicateFilesRemover(bool useConsole, FileListModel fileList, List<string> priorityList)
    {
        _useConsole = useConsole;
        _fileList = fileList;
        _priorityList = priorityList;
    }

    internal bool Run()
    {
        Console.WriteLine("Delete duplicate Files");

        StShared.ConsoleWriteInformationLine("Remove Duplicate Files", _useConsole);
        foreach (var kvp in _fileList.DuplicateFilesStorage) kvp.Value.RemoveDuplicates(_priorityList);

        StShared.ConsoleWriteInformationLine("Finish", _useConsole);

        return true;
    }
}