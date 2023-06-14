using System;
using System.IO;
using System.Linq;
using CompressionManagement;
using LibToolActions.BackgroundTasks;
using Microsoft.Extensions.Logging;

namespace LibApAgentData.StepCommands;

public sealed class UnZipOnPlaceCommand : ProcessesToolAction
{
    private readonly string _pathWithZips;
    private readonly bool _withSubFolders;

    public UnZipOnPlaceCommand(ILogger logger, bool useConsole, ProcessManager processManager, string pathWithZips,
        bool withSubFolders, int procLineId) : base(logger, useConsole, processManager, "UnZip On Place", procLineId)
    {
        _pathWithZips = pathWithZips;
        _withSubFolders = withSubFolders;
    }

    protected override bool RunAction()
    {
        Logger.LogInformation("Checking parameters...");

        //უნდა შემოწმდეს არსებობს თუ არა ფოლდერი _unZipOnPlaceStep.WithSubFolders
        //თუ არ არსებობს ვჩერდებით

        var curDir = new DirectoryInfo(_pathWithZips);
        return ProcessFolder(curDir, _withSubFolders);
    }

    private bool ProcessFolder(DirectoryInfo curDir, bool useSubFolders = true)
    {
        Console.WriteLine($"Process Folder {curDir.FullName}");

        if (useSubFolders)
            if (curDir.GetDirectories().Any(dir => !ProcessFolder(dir)))
                return false;

        foreach (var file in curDir.GetFiles())
        {
            if (file.Extension.ToLower() != ".zip")
                continue;
            var zipFileName = Path.GetFileNameWithoutExtension(file.Name);
            var i = 0;
            while (Directory.Exists(Path.Combine(curDir.FullName, GetNewFolderName(zipFileName, i))))
                i++;

            var newDir =
                Directory.CreateDirectory(Path.Combine(curDir.FullName, GetNewFolderName(zipFileName, i)));
            var archiver = new ZipClassArchiver(Logger, UseConsole, file.Extension);

            Console.WriteLine($"Unzip {file.FullName}");

            if (!archiver.ArchiveToPath(file.FullName, newDir.FullName))
                return false;

            file.Delete();
        }

        return true;
    }

    private string GetNewFolderName(string zipFileName, int i)
    {
        return $"{zipFileName}{(i == 0 ? "" : $"({i})")}";
    }
}