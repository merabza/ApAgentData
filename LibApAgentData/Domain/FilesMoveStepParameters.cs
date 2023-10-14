using System;
using System.Collections.Generic;
using FileManagersMain;
using LibApAgentData.Models;
using LibApAgentData.StepCommands;
using LibFileParameters.Models;
using Microsoft.Extensions.Logging;
using SystemToolsShared;

namespace LibApAgentData.Domain;

public sealed class FilesMoveStepParameters
{
    private FilesMoveStepParameters(FileStorageData sourceFileStorage, bool sourceIsLocal,
        FileStorageData destinationFileStorage, bool destinationIsLocal, ExcludeSet excludeSet,
        ExcludeSet? deleteDestinationFilesSet, FileManager sourceFileManager, FileManager destinationFileManager,
        EMoveMethod useMethod, ReplacePairsSet? replacePairsSet, string moveFolderMask, string uploadTempExtension,
        string downloadTempExtension, int maxFolderCount, bool createFolderWithDateTime, List<string> priorityPoints)
    {
        SourceFileStorage = sourceFileStorage;
        SourceIsLocal = sourceIsLocal;
        DestinationFileStorage = destinationFileStorage;
        DestinationIsLocal = destinationIsLocal;
        ExcludeSet = excludeSet;
        DeleteDestinationFilesSet = deleteDestinationFilesSet;
        SourceFileManager = sourceFileManager;
        DestinationFileManager = destinationFileManager;
        UseMethod = useMethod;
        ReplacePairsSet = replacePairsSet;
        MoveFolderMask = moveFolderMask;
        UploadTempExtension = uploadTempExtension;
        DownloadTempExtension = downloadTempExtension;
        MaxFolderCount = maxFolderCount;
        CreateFolderWithDateTime = createFolderWithDateTime;
        PriorityPoints = priorityPoints;
    }

    public FileStorageData SourceFileStorage { get; }
    public bool SourceIsLocal { get; }
    public FileStorageData DestinationFileStorage { get; }
    public bool DestinationIsLocal { get; }
    public ExcludeSet ExcludeSet { get; }
    public ExcludeSet? DeleteDestinationFilesSet { get; }
    public FileManager SourceFileManager { get; }
    public FileManager DestinationFileManager { get; }
    public EMoveMethod UseMethod { get; }
    public ReplacePairsSet? ReplacePairsSet { get; }
    public string MoveFolderMask { get; }
    public string UploadTempExtension { get; }
    public string DownloadTempExtension { get; }
    public int MaxFolderCount { get; } //მაქსიმუმ რამდენი ფოლდერის სახელი შეუნარჩუნდეს ფაილს

    public bool
        CreateFolderWithDateTime { get; } //შევქმნათ თუ არა თარიღიანი და დროიანი ცალკე ფოლდერი ერთი სესიისათვის

    //public Dictionary<string, string> PriorityFolderPaths { get; set; } //პრიორიტეტული ფოლდერების ჩამონათვალი. (სახელი გამოიყენება ნიღბის კოდად)
    public List<string> PriorityPoints { get; } //პრიორიტეტული ფოლდერების ჩამონათვალი.


    public static FilesMoveStepParameters? Create(ILogger logger, bool useConsole, string? sourceFileStorageName,
        string? destinationFileStorageName, string? excludeSetName, string? deleteDestinationFilesSetName,
        string? replacePairsSetName, string moveFolderMask, string uploadTempExtension, string downloadTempExtension,
        FileStorages fileStorages, ExcludeSets excludeSets, ReplacePairsSets replacePairsSets, int maxFolderCount,
        bool createFolderWithDateTime, List<string> priorityPoints)
    {
        if (string.IsNullOrWhiteSpace(sourceFileStorageName))
        {
            StShared.WriteErrorLine("sourceFileStorageName does not specified for Files Sync step", useConsole, logger);
            return null;
        }

        var sourceFileStorage = fileStorages.GetFileStorageDataByKey(sourceFileStorageName);

        if (sourceFileStorage == null)
        {
            StShared.WriteErrorLine("Source File Storage not specified for Files Backup step", useConsole, logger);
            return null;
        }

        if (string.IsNullOrWhiteSpace(sourceFileStorage.FileStoragePath))
        {
            StShared.WriteErrorLine("sourceFileStorage.FileStoragePath does not specified for Files Sync step",
                useConsole, logger);
            return null;
        }

        if (string.IsNullOrWhiteSpace(destinationFileStorageName))
        {
            StShared.WriteErrorLine("destinationFileStorageName does not specified for Files Sync step", useConsole,
                logger);
            return null;
        }

        var destinationFileStorage = fileStorages.GetFileStorageDataByKey(destinationFileStorageName);

        if (destinationFileStorage == null)
        {
            StShared.WriteErrorLine("destination File Storage not specified for Files Backup step", useConsole,
                logger);
            return null;
        }

        if (string.IsNullOrWhiteSpace(destinationFileStorage.FileStoragePath))
        {
            StShared.WriteErrorLine("destinationFileStorage.FileStoragePath does not specified for Files Sync step",
                useConsole, logger);
            return null;
        }

        var sourceIsLocal = sourceFileStorage.IsFileSchema();
        if (sourceIsLocal is null)
        {
            StShared.WriteErrorLine("could not be determined source is File Schema or not", useConsole, logger);
            return null;
        }


        var destinationIsLocal = destinationFileStorage.IsFileSchema();
        if (destinationIsLocal is null)
        {
            StShared.WriteErrorLine("could not be determined destination is File Schema or not", useConsole, logger);
            return null;
        }

        if (!sourceIsLocal.Value && !destinationIsLocal.Value)
        {
            StShared.WriteErrorLine("At Least one file storage must be a local storage", useConsole, logger);
            return null;
        }

        if (string.IsNullOrWhiteSpace(excludeSetName))
        {
            StShared.WriteErrorLine("excludeSetName does not specified for Files Sync step", useConsole,
                logger);
            return null;
        }

        var excludeSet = excludeSets.GetExcludeSetByKey(excludeSetName);

        if (excludeSet == null)
        {
            StShared.WriteErrorLine("excludeSet does not created for Files Backup step", useConsole,
                logger);
            return null;
        }

        ExcludeSet? deleteDestinationFilesSet = null;
        if (!string.IsNullOrWhiteSpace(deleteDestinationFilesSetName))
            deleteDestinationFilesSet = excludeSets.GetExcludeSetByKey(deleteDestinationFilesSetName);

        StShared.ConsoleWriteInformationLine($"Source is From {sourceFileStorage.FileStoragePath}", useConsole, logger);

        FileManager? sourceFileManager;
        //თუ წყარო ლოკალურია
        if (sourceIsLocal.Value)
            //შევქმნათ ლოკალური გამგზავნი ფაილ მენეჯერი
            sourceFileManager =
                FileManagersFabric.CreateFileManager(useConsole, logger, sourceFileStorage.FileStoragePath);
        else
            //თუ წყარო მოშორებულია
            //შევქმნათ ჩამოსატვირთი ფაილ მენეჯერი
            sourceFileManager = FileManagersFabricExt.CreateFileManager(useConsole, logger,
                destinationFileStorage.FileStoragePath, sourceFileStorage);

        if (sourceFileManager == null)
        {
            StShared.WriteErrorLine("sourceFileManager does not created for Files Backup step", useConsole,
                logger);
            return null;
        }

        Console.WriteLine($"Destination is {destinationFileStorage.FileStoragePath}");

        FileManager? destinationFileManager;
        //თუ მიზანი ლოკალურია
        if (destinationIsLocal.Value)
            //შევქმნათ ლოკალური მიმღები ფაილ მენეჯერი
            destinationFileManager =
                FileManagersFabric.CreateFileManager(useConsole, logger, destinationFileStorage.FileStoragePath);
        else
            //თუ მიზანი მოშორებულია
            //შევქმნათ ასატვირთი ფაილ მენეჯერი
            destinationFileManager = FileManagersFabricExt.CreateFileManager(useConsole, logger,
                sourceFileStorage.FileStoragePath, destinationFileStorage);

        if (destinationFileManager == null)
        {
            StShared.WriteErrorLine("destinationFileManager does not created for Files Backup step", useConsole,
                logger);
            return null;
        }

        EMoveMethod useMethod;
        //თუ წყარო მოშორებულია
        if (!sourceIsLocal.Value)
            useMethod = EMoveMethod.Download;
        else
            useMethod = destinationIsLocal.Value ? EMoveMethod.Local : EMoveMethod.Upload;

        ReplacePairsSet? replacePairsSet = null;
        if (!string.IsNullOrWhiteSpace(replacePairsSetName))
            replacePairsSet = replacePairsSets.GetReplacePairsSetByKey(replacePairsSetName);


        return new FilesMoveStepParameters(sourceFileStorage, sourceIsLocal.Value, destinationFileStorage,
            destinationIsLocal.Value, excludeSet, deleteDestinationFilesSet, sourceFileManager, destinationFileManager,
            useMethod, replacePairsSet, moveFolderMask, uploadTempExtension, downloadTempExtension, maxFolderCount,
            createFolderWithDateTime, priorityPoints);
    }
}