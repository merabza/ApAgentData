using System.Collections.Generic;
using System.IO;
using System.Linq;
using ApAgentData.LibApAgentData.StepCommands;
using ConnectTools;
using FileManagersMain;
using LibFileParameters.Models;
using Microsoft.Extensions.Logging;

namespace ApAgentData.LibApAgentData.FolderProcessors;

public sealed class MoveFiles : CopyMoveFiles
{
    private readonly List<string> _checkedFolders = [];
    private readonly int _maxFolderCount;
    private readonly string? _moveFolderName;

    public MoveFiles(ILogger logger, FileManager sourceFileManager, FileManager destinationFileManager,
        string? moveFolderName, EMoveMethod useMethod, string uploadTempExtension, string downloadTempExtension,
        ExcludeSet excludeSet, int maxFolderCount, int destinationFileMaxLength) : base("Move files",
        "Move files from one place to another", sourceFileManager, null, true, excludeSet, true, true,
        destinationFileManager, logger, useMethod, uploadTempExtension, downloadTempExtension, destinationFileMaxLength)
    {
        _moveFolderName = moveFolderName?.Trim();
        _maxFolderCount = maxFolderCount;
    }

    protected override bool ProcessOneFile(string? afterRootPath, MyFileInfo file)
    {
        var dirNames = afterRootPath?.Split(FileManager.DirectorySeparatorChar).TakeLast(_maxFolderCount)
            .Select(s => s.Trim()).ToList() ?? [];
        var destinationAfterRootPath = CheckDestinationDirs(dirNames);

        var preparedFileName = file.FileName.PrepareFileName().Trim();
        var extension = Path.GetExtension(preparedFileName).Trim();
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(preparedFileName).Trim();

        var i = 0;
        while (DestinationFileManager.FileExists(destinationAfterRootPath,
                   fileNameWithoutExtension.GetNewFileNameWithMaxLength(i, extension, FileMaxLength)))
        {
            i++;
        }

        preparedFileName = fileNameWithoutExtension.GetNewFileNameWithMaxLength(i, extension, FileMaxLength);

        return ProcessOneFile(UseMethod, afterRootPath, file, destinationAfterRootPath, preparedFileName, true);
    }

    private string? CheckDestinationDirs(IEnumerable<string> dirNames)
    {
        var afterRootPath = _moveFolderName;
        foreach (var dir in dirNames)
        {
            var validDir = dir;
            //როცა ფოლდერის სახელის ბოლოში არის წერტილი, ეს ცუდად მოქმედებს შემდეგ პროცესებზე
            //FTP-ს მხარეს გადაწერა ხერხდება, მაგრამ FTP-დან ლინუქსზე ვეღარ.
            //ვინდოუსი ჭკვიანურად იქცევა და ასეთი ფოლდერის შექმნისას თვითონ აჭრის ბოლო წერტილებს.
            //თუმცა ეს პროგრამა ლინუქსზეც ეშვება. ამიტომ აქ ვითვალისწინებ ამ პრობლემას.
            //თუ უწერტილო ფოლდერის სახელს დაემთხვა პრობლემა არაა
            while (validDir.EndsWith('.'))
            {
                validDir = validDir.TrimEnd('.');
            }

            if (validDir.Length == 0) //თუ ფოლდერის სახელი მხოლოდ წერტილებისაგან შედგებოდა, ასეთი ფოლდერი ვერ შეიქმნება
            {
                return null;
            }

            var forCreateDirPart = DestinationFileManager.PathCombine(afterRootPath, dir);
            if (!_checkedFolders.Contains(forCreateDirPart))
            {
                if (!DestinationFileManager.CareCreateDirectory(afterRootPath, dir, true))
                {
                    return null;
                }

                _checkedFolders.Add(forCreateDirPart);
            }

            afterRootPath = forCreateDirPart;
        }

        return afterRootPath;
    }
}
