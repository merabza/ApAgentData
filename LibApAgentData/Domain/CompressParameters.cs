﻿using CompressionManagement;
using FileManagersMain;
using LibFileParameters.Models;
using Microsoft.Extensions.Logging;
using SystemToolsShared;

namespace LibApAgentData.Domain;

public sealed class CompressParameters
{
    private CompressParameters(string workPath, Archiver archiver, FileManager workFileManager,
        string archiveTempExtension)
    {
        WorkPath = workPath;
        Archiver = archiver;
        WorkFileManager = workFileManager;
        ArchiveTempExtension = archiveTempExtension;
    }

    public string WorkPath { get; }
    public Archiver Archiver { get; }
    public FileManager WorkFileManager { get; }
    public string ArchiveTempExtension { get; }

    public static CompressParameters? Create(ILogger logger, bool useConsole, string? workPath,
        string? archiveTempExtension, ArchiverData archiverData)
    {
        if (string.IsNullOrWhiteSpace(workPath))
        {
            StShared.WriteErrorLine("workPath does not specified", useConsole, logger);
            return null;
        }

        if (string.IsNullOrWhiteSpace(archiverData.FileExtension))
        {
            StShared.WriteErrorLine("archiverData.FileExtension does not specified", useConsole, logger);
            return null;
        }

        var archiver = ArchiverFabric.CreateArchiverByType(useConsole, logger, archiverData.Type,
            archiverData.CompressProgramPatch, archiverData.DecompressProgramPatch,
            archiverData.FileExtension.AddNeedLeadPart("."));

        if (archiver is null)
        {
            StShared.WriteErrorLine("CompressParameters: archiver does not created", useConsole, logger);
            return null;
        }

        var workFileManager = FileManagersFabric.CreateFileManager(useConsole, logger, workPath);

        if (workFileManager is null)
        {
            StShared.WriteErrorLine("CompressParameters: workFileManager does not created", useConsole, logger);
            return null;
        }

        if (string.IsNullOrWhiteSpace(archiveTempExtension))
        {
            StShared.WriteErrorLine("tempExtension does not specified", useConsole, logger);
            return null;
        }

        return new CompressParameters(workPath, archiver, workFileManager, archiveTempExtension.AddNeedLastPart("."));
    }
}