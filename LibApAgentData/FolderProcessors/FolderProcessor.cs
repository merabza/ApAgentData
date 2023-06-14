using System;
using System.Linq;
using ConnectTools;
using FileManagersMain;
using LibFileParameters.Models;
using SystemToolsShared;

namespace LibApAgentData.FolderProcessors;

public /*open*/ class FolderProcessor
{
    private readonly bool _deleteEmptyFolders;
    private readonly string _description;

    private readonly string? _fileSearchPattern;

    //private readonly ILogger _logger;
    //private readonly bool _useConsole;
    private readonly string _name;
    private readonly RecursiveParameters? _startRecursiveParameters;
    private readonly bool _useSubFolders;
    protected readonly ExcludeSet? ExcludeSet;
    protected readonly FileManager FileManager;

    protected FolderProcessor(
        //ILogger logger, bool useConsole,
        string name, string description, FileManager fileManager,
        string? fileSearchPattern, bool deleteEmptyFolders, RecursiveParameters? startRecursiveParameters,
        ExcludeSet? excludeSet, bool useSubFolders = true)
    {
        //_logger = logger;
        //_useConsole = useConsole;
        _name = name;
        _description = description;
        FileManager = fileManager;
        _fileSearchPattern = fileSearchPattern;
        _deleteEmptyFolders = deleteEmptyFolders;
        _startRecursiveParameters = startRecursiveParameters;
        ExcludeSet = excludeSet;
        _useSubFolders = useSubFolders;
    }

    protected virtual bool CheckParameters()
    {
        //if (FileManager is null)
        //{
        //    StShared.WriteErrorLine("FileManager for FolderProcessor is null", _useConsole, _logger);
        //    return false;
        //}

        return true;
    }

    public bool Run()
    {
        if (!CheckParameters())
            return false;
        Console.WriteLine(_description);
        var toReturn = ProcessFolder(_startRecursiveParameters);
        Finish();
        return toReturn;
    }

    protected virtual void Finish()
    {
    }

    private bool ProcessFolder(RecursiveParameters? recursiveParameters, string? afterRootPath = null)
    {
        Console.WriteLine($"({_name}) Process Folder {afterRootPath}");

        if (_useSubFolders)
        {
            var reloadFolders = true;
            while (reloadFolders)
            {
                var folderNames = FileManager.GetFolderNames(afterRootPath, null)
                    .Where(x => !x.StartsWith("#")).OrderBy(o => o).ToList();
                reloadFolders = false;
                foreach (var folderName in folderNames)
                {
                    var (success, folderNameChanged) =
                        ProcessOneFolder(afterRootPath, folderName, recursiveParameters);
                    if (folderNameChanged)
                    {
                        reloadFolders = true;
                        break;
                    }

                    if (!success)
                        return false;

                    var folderAfterRootFullName = FileManager.PathCombine(afterRootPath, folderName);
                    if (!ProcessFolder(CountNextRecursiveParameters(recursiveParameters, folderName),
                            folderAfterRootFullName))
                        return false;
                }
            }
        }

        if (!ProcessFiles(afterRootPath, recursiveParameters))
            return false;

        if (!_deleteEmptyFolders)
            return true;

        //შევამოწმოთ დაცარიელდა თუ არა ფოლდერი და თუ დაცარიელდა, წავშალოთ
        if (afterRootPath != null && FileManager.IsFolderEmpty(afterRootPath))
            FileManager.DeleteDirectory(afterRootPath);

        return true;
    }

    protected virtual RecursiveParameters? CountNextRecursiveParameters(RecursiveParameters? recursiveParameters,
        string folderName)
    {
        return null;
    }

    protected virtual (bool, bool) ProcessOneFolder(string? afterRootPath, string folderName,
        RecursiveParameters? recursiveParameters = null)
    {
        return (true, false);
    }

    protected virtual bool ProcessOneFile(string? afterRootPath, MyFileInfo file,
        RecursiveParameters? recursiveParameters = null)
    {
        return true;
    }

    private bool ProcessFiles(string? afterRootPath, RecursiveParameters? recursiveParameters = null)
    {
        return FileManager.GetFilesWithInfo(afterRootPath, _fileSearchPattern).OrderBy(o => o.FileName)
            .Where(file =>
                ExcludeSet == null ||
                !ExcludeSet.NeedExclude(FileManager.PathCombine(afterRootPath, file.FileName)))
            .All(file => ProcessOneFile(afterRootPath, file, recursiveParameters));
    }


    protected static bool NeedExclude(string name, string[] excludes)
    {
        var haveExclude = excludes is { Length: > 0 };
        return haveExclude && excludes.Any(name.FitsMask);
    }
}