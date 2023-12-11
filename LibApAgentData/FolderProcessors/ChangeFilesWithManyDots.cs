﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ConnectTools;
using FileManagersMain;
// ReSharper disable ConvertToPrimaryConstructor

namespace LibApAgentData.FolderProcessors;

public sealed class ChangeFilesWithRestrictPatterns : FolderProcessor
{
    private readonly Dictionary<string, string> _replaceSet;

    public ChangeFilesWithRestrictPatterns(FileManager destinationFileManager, Dictionary<string, string> replaceSet) :
        base("Restrict Patterns", "Change Files With Restrict Patterns", destinationFileManager, null, false, null)
    {
        _replaceSet = replaceSet;
    }

    protected override bool CheckParameters()
    {
        if (_replaceSet is { Count: > 0 })
            return true;
        Console.WriteLine("Replace Set patterns not specified");
        return false;
    }

    protected override bool ProcessOneFile(string? afterRootPath, MyFileInfo file)
    {
        var haveReplace = true;
        var newFileName = file.FileName;
        while (haveReplace)
        {
            haveReplace = false;
            foreach (var kvp in _replaceSet.Where(kvp => newFileName.Contains(kvp.Key)))
            {
                newFileName = newFileName.Replace(kvp.Key, kvp.Value);
                haveReplace = true;
                break;
            }
        }

        if (newFileName == file.FileName)
            return true;

        //ვიპოვოთ newFileName არის თუ არა destinationFileManager-ის ფოლდერში
        //თუ არ არსებობს, შევუცვალოთ სახელი ფაილს
        //თუ არსებობს, გამოვიყენოთ ბოლოში ციფრების მიწერა, სანამ თავისუფალ სახელს არ ვიპოვით

        //შევუცვალოთ სახელი ფაილს fileName დან newFileName-კენ

        var extension = Path.GetExtension(newFileName);
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(newFileName);


        var i = 0;
        while (FileManager.FileExists(afterRootPath,
                   fileNameWithoutExtension.GetNewFileName(i, extension)))
            i++;

        return FileManager.RenameFile(afterRootPath, file.FileName,
            fileNameWithoutExtension.GetNewFileName(i, extension));
    }
}