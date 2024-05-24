using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using DatabasesManagement;
using DbTools;
using FileManagersMain;
using LibApAgentData.Models;
using LibApiClientParameters;
using LibDatabaseParameters;
using LibFileParameters.Models;
using Microsoft.Extensions.Logging;
using SystemToolsShared;

namespace LibApAgentData.Domain;

public sealed class DatabaseBackupStepParameters
{
    private DatabaseBackupStepParameters(IDatabaseManager agentClient, string localPath,
        FileManager localWorkFileManager, EBackupType backupType, EDatabaseSet databaseSet, List<string> databaseNames,
        FileStorageData downloadFileStorageData, FileStorageData uploadFileStorageData,
        SmartSchema downloadSideSmartSchema, SmartSchema localSmartSchema, FileManager downloadFileManager,
        DatabaseBackupParametersDomain dbBackupParameters, int downloadProcLineId, int compressProcLineId,
        CompressParameters? compressParameters, UploadParameters uploadParameters)
    {
        AgentClient = agentClient;
        LocalPath = localPath;
        LocalWorkFileManager = localWorkFileManager;
        BackupType = backupType;
        DatabaseSet = databaseSet;
        DatabaseNames = databaseNames;
        DownloadFileStorageData = downloadFileStorageData;
        UploadFileStorageData = uploadFileStorageData;
        DownloadSideSmartSchema = downloadSideSmartSchema;
        LocalSmartSchema = localSmartSchema;
        DownloadFileManager = downloadFileManager;
        DbBackupParameters = dbBackupParameters;
        DownloadProcLineId = downloadProcLineId;
        CompressProcLineId = compressProcLineId;
        CompressParameters = compressParameters;
        UploadParameters = uploadParameters;
    }

    public IDatabaseManager AgentClient { get; }
    public string LocalPath { get; } //ლოკალური ფოლდერი ბექაპების მისაღებად
    public FileManager LocalWorkFileManager { get; } //ლოკალური ფოლდერის მენეჯერი
    public EBackupType BackupType { get; }
    public EDatabaseSet DatabaseSet { get; } //ბაზების სიმრავლე, რომლისთვისაც უნდა გაეშვას ეს პროცესი.
    public List<string> DatabaseNames { get; }
    public FileStorageData DownloadFileStorageData { get; }
    public FileStorageData UploadFileStorageData { get; }
    public SmartSchema DownloadSideSmartSchema { get; }
    public SmartSchema LocalSmartSchema { get; }
    public FileManager DownloadFileManager { get; }
    public DatabaseBackupParametersDomain DbBackupParameters { get; }
    public int DownloadProcLineId { get; }
    public int CompressProcLineId { get; }
    public CompressParameters? CompressParameters { get; }
    public UploadParameters UploadParameters { get; }

    public static DatabaseBackupStepParameters? Create(ILogger logger, IHttpClientFactory httpClientFactory,
        bool useConsole, string? webAgentName, ApiClients apiClients, string? databaseServerConnectionName,
        DatabaseServerConnections databaseServerConnections, string? localPath,
        DatabaseBackupParametersModel? databaseBackupParameters, string? dbServerSideBackupPath,
        EDatabaseSet databaseSet, List<string> databaseNames, string? fileStorageName, string? uploadFileStorageName,
        string? smartSchemaName, string? localSmartSchemaName, string? uploadSmartSchemaName, string? archiverName,
        FileStorages fileStorages, SmartSchemas smartSchemas, Archivers archivers, int downloadProcLineId,
        int compressProcLineId, int uploadProcLineId, string? archiveTempExtension, string? uploadTempExtension)
    {
        if (string.IsNullOrWhiteSpace(localPath))
        {
            StShared.WriteErrorLine("localPath is not specified", useConsole, logger);
            return null;
        }

        var localWorkFileManager = FileManagersFabric.CreateFileManager(useConsole, logger, localPath);

        if (localWorkFileManager is null)
        {
            StShared.WriteErrorLine("FileManager for localPath does not created", useConsole, logger);
            return null;
        }

        var agentClient = DatabaseAgentClientsFabric.CreateDatabaseManager(useConsole, logger, httpClientFactory,
            webAgentName, apiClients, databaseServerConnectionName, databaseServerConnections, null, null,
            CancellationToken.None).Result;

        if (agentClient is null)
        {
            StShared.WriteErrorLine($"DatabaseManagementClient does not created for webAgent {webAgentName}",
                useConsole, logger);
            return null;
        }

        if (databaseBackupParameters is null)
        {
            StShared.WriteErrorLine("databaseBackupParameters does not specified",
                useConsole, logger);
            return null;
        }


        if (string.IsNullOrWhiteSpace(fileStorageName))
        {
            StShared.WriteErrorLine("fileStorageName is not specified", useConsole, logger);
            return null;
        }

        var downloadFileStorageData = fileStorages.GetFileStorageDataByKey(fileStorageName);

        if (downloadFileStorageData is null)
        {
            StShared.WriteErrorLine("downloadFileStorageData did not created", useConsole, logger);
            return null;
        }


        if (string.IsNullOrWhiteSpace(uploadFileStorageName))
        {
            StShared.WriteErrorLine("uploadFileStorageName is not specified", useConsole, logger);
            return null;
        }

        var uploadFileStorageData = fileStorages.GetFileStorageDataByKey(uploadFileStorageName);

        if (uploadFileStorageData is null)
        {
            StShared.WriteErrorLine("uploadFileStorageData did not created", useConsole, logger);
            return null;
        }


        if (string.IsNullOrWhiteSpace(smartSchemaName))
        {
            StShared.WriteErrorLine("smartSchemaName is not specified", useConsole, logger);
            return null;
        }

        var downloadSideSmartSchema = smartSchemas.GetSmartSchemaByKey(smartSchemaName);

        if (downloadSideSmartSchema is null)
        {
            StShared.WriteErrorLine("downloadSideSmartSchema did not created", useConsole, logger);
            return null;
        }


        if (string.IsNullOrWhiteSpace(localSmartSchemaName))
        {
            StShared.WriteErrorLine("localSmartSchemaName is not specified", useConsole, logger);
            return null;
        }

        var localSmartSchema = smartSchemas.GetSmartSchemaByKey(localSmartSchemaName);

        if (localSmartSchema is null)
        {
            StShared.WriteErrorLine("localSmartSchema did not created", useConsole, logger);
            return null;
        }


        if (string.IsNullOrWhiteSpace(uploadSmartSchemaName))
        {
            StShared.WriteErrorLine("uploadSmartSchemaName is not specified", useConsole, logger);
            return null;
        }

        var uploadSmartSchema = smartSchemas.GetSmartSchemaByKey(uploadSmartSchemaName);

        if (uploadSmartSchema is null)
        {
            StShared.WriteErrorLine("uploadSmartSchema did not created", useConsole, logger);
            return null;
        }


        //წავშალოთ ზედმეტი ფაილები მონაცემთა ბაზის მხარეს
        var downloadFileManager =
            FileManagersFabricExt.CreateFileManager(useConsole, logger, null, downloadFileStorageData, true);

        if (downloadFileManager is null)
        {
            StShared.WriteErrorLine("downloadFileManager did not created", useConsole, logger);
            return null;
        }

        CompressParameters? compressParameters = null;
        if (string.IsNullOrWhiteSpace(archiverName))
        {
            StShared.WriteWarningLine("archiverName is not specified", useConsole, logger);
        }
        else
        {
            var archiver = archivers.GetArchiverDataByKey(archiverName);

            if (archiver is null)
            {
                StShared.WriteWarningLine($"archiver does not created for {archiverName}", useConsole, logger);
                return null;
            }

            compressParameters =
                CompressParameters.Create(logger, useConsole, localPath, archiveTempExtension, archiver);
            if (compressParameters is null)
            {
                StShared.WriteWarningLine($"compressParameters does not created for {archiverName}", useConsole,
                    logger);
                return null;
            }
        }


        var dbBackupParameters =
            DatabaseBackupParametersDomain.Create(databaseBackupParameters, dbServerSideBackupPath);

        if (dbBackupParameters is null)
        {
            StShared.WriteErrorLine("Backup Parameters does not created", useConsole, logger);
            return null;
        }

        var uploadParameters = UploadParameters.Create(logger, useConsole, localPath,
            uploadFileStorageData, uploadSmartSchema, uploadTempExtension, uploadProcLineId);

        if (uploadParameters is not null)
            return new DatabaseBackupStepParameters(agentClient, localPath, localWorkFileManager,
                databaseBackupParameters.BackupType, databaseSet, databaseNames, downloadFileStorageData,
                uploadFileStorageData, downloadSideSmartSchema, localSmartSchema, downloadFileManager,
                dbBackupParameters, downloadProcLineId, compressProcLineId, compressParameters, uploadParameters);

        StShared.WriteErrorLine("uploadParameters does not created", useConsole, logger);
        return null;
    }
}