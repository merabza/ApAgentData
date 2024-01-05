using System;
using LibApAgentData.Models;
using SystemToolsShared;

namespace LibApAgentData.SubCommands;

public sealed class MultiDuplicatesFinder
{
    private readonly bool _useConsole;

    // ReSharper disable once ConvertToPrimaryConstructor
    public MultiDuplicatesFinder(bool useConsole, FileListModel fileList)
    {
        _useConsole = useConsole;
        FileList = fileList;
    }

    public FileListModel FileList { get; }

    internal bool Run()
    {
        Console.WriteLine("Find Multi duplicate Files");

        foreach (var kvp in FileList.DuplicateFilesStorage) kvp.Value.CountMultiDuplicates();

        StShared.ConsoleWriteInformationLine(null, _useConsole, "Finish");

        return true;
    }
}