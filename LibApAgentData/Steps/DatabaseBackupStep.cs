using System.Collections.Generic;
using LibApAgentData.Domain;
using LibApAgentData.Models;
using LibApAgentData.StepCommands;
using LibApiClientParameters;
using LibDatabaseParameters;
using LibFileParameters.Models;
using LibToolActions.BackgroundTasks;
using Microsoft.Extensions.Logging;
using SystemToolsShared;

// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace LibApAgentData.Steps;

public sealed class DatabaseBackupStep : JobStep
{
    //თუ ბაზასთან დასაკავშირებლად ვიყენებთ პირდაპირ კავშირს, მაშინ ვებაგენტი აღარ გამოიყენება და პირიქით
    public string? DatabaseServerConnectionName { get; set; } //ბაზასთან დაკავშირების პარამეტრების ჩანაწერის სახელი
    public string? DatabaseWebAgentName { get; set; } //შეიძლება ბაზასთან დასაკავშირებლად გამოვიყენოთ ვებაგენტი

    public DatabaseBackupParametersModel? DatabaseBackupParameters { get; set; }

    public EDatabaseSet DatabaseSet { get; set; } //ბაზების სიმრავლე, რომლისთვისაც უნდა გაეშვას ეს პროცესი.

    //თუ DatabaseSet-ის მნიშვნელობაა DatabasesBySelection, მაშინ მონაცემთა ბაზების სახელები უნდა ავიღოთ ქვემოთ მოცემული სიიდან
    // ReSharper disable once CollectionNeverUpdated.Global
    public List<string> DatabaseNames { get; set; } = new();

    //ბექაპირება
    //ბექაპის ფოლდერის გზა. ფოლდერი, სადაც უნდა შეიქმნას ბექაპის ფაილი სერვერის მხარეს.
    //ეს პარამეტრი გამოიყენება მხოლოდ იმ შემთხვევაში, თუ ბაზასთან დასაკავშირებლად არ ვიყენებთ ვებაგენტს
    public string? DbServerSideBackupPath { get; set; }

    //ბაზის სერვერის მხარე
    public string?
        SmartSchemaName
    {
        get;
        set;
    } //ჭკვიანი სქემის სახელი. გამოიყენება ძველი დასატოვებელი და წასაშლელი ფაილების განსასაზღვრად. (ეს მონაცემთა ბაზის სერვერის მხარეს)

    public string?
        FileStorageName
    {
        get;
        set;
    } //ფაილსაცავის სახელი, რომელიც საშუალებას იძლევა ხელმისაწვდომი გახდეს ახლადგაკეთებული ბექაპი

    //ჩამოტვირთვა და ლოკალური მხარე
    public int DownloadProcLineId { get; set; } //ჩამოტვირთვის პროცესის ხაზის იდენტიფიკატორი
    public string? LocalPath { get; set; } //ლოკალური ფოლდერი ბექაპების მისაღებად

    public string?
        LocalSmartSchemaName
    {
        get;
        set;
    } //ჭკვიანი სქემის სახელი. გამოიყენება ძველი დასატოვებელი და წასაშლელი ფაილების განსასაზღვრად. (ეს ლოკალური ფოლდერის მხარეს)

    //დაკუმშვა
    public string? ArchiverName { get; set; } //ფაილის დასაკუმშად გამოსაყენებელი არქივატორის სახელი.

    public int CompressProcLineId { get; set; } //დაკუმშვის პროცესის ხაზის იდენტიფიკატორი

    //ატვირთვა რეზერვაციისათვის
    public string?
        UploadFileStorageName
    {
        get;
        set;
    } //ფაილსაცავის სახელი, რომელიც გამოიყენება რეზერვაციისათვის ბექაპების ასატვირთად

    public int UploadProcLineId { get; set; } //ატვირთვის პროცესის ხაზის იდენტიფიკატორი

    public string?
        UploadSmartSchemaName
    {
        get;
        set;
    } //ჭკვიანი სქემის სახელი. გამოიყენება ძველი დასატოვებელი და წასაშლელი ფაილების განსასაზღვრად. (ეს რეზერვაციის ფაილსაცავის მხარეს)

    //პირველ რიგში უნდა გაეშვას ინფორმაციის შემგროვებელი,
    //რომელიც დაადგენს რომელი ბაზების დაბექაპება არის საჭირო.
    //შემდეგ ციკლში თითოეული ბაზა დაბექაპდეს,
    //დაბექაპების გარდა გვაქვს დამატებითი სამუშაოები შესასრულებელი
    //1. მოქაჩვა, 2. დაკუმშვა, 3. ფაილსაცავში ატვირთვა
    //ამ სამუშაოებისათვის ცალკე პროცესების შექმნა ხდება მხოლოდ იმ შემთხვევაში, თუ ისინი სხვა პროცესის ხაზშია.
    //თუ რომელიმე სამუშაოსათვის შეიქმნა ცალკე პროცესი, შემდგომი სამუშაოების გაშვება ამ პროცესზე იქნება დამოკიდებული, მიუხედავად ხაზების ნომრებისა
    //თითოეული სამუშაო ახალ პროცესს ქმნის მხოლოდ იმ შემთხვევაში, თუ შემდეგი სამუშაოს პროცესის ხაზი არ ემთხვევა მიმდინარესას.
    public override ProcessesToolAction? GetToolAction(ILogger logger, bool useConsole, ProcessManager processManager,
        ApAgentParameters parameters, string procLogFilesFolder)
    {
        var par = DatabaseBackupStepParameters.Create(logger, useConsole,
            DatabaseWebAgentName, new ApiClients(parameters.ApiClients), DatabaseServerConnectionName,
            new DatabaseServerConnections(parameters.DatabaseServerConnections), LocalPath, DatabaseBackupParameters,
            DbServerSideBackupPath, DatabaseSet, DatabaseNames, FileStorageName, UploadFileStorageName, SmartSchemaName,
            LocalSmartSchemaName, UploadSmartSchemaName, ArchiverName, new FileStorages(parameters.FileStorages),
            new SmartSchemas(parameters.SmartSchemas), new Archivers(parameters.Archivers), DownloadProcLineId,
            CompressProcLineId, UploadProcLineId, parameters.GetArchivingFileTempExtension(),
            parameters.GetUploadFileTempExtension());

        if (par is not null)
            return new DatabaseBackupStepCommand(useConsole, logger, processManager, this, par,
                parameters.GetDownloadFileTempExtension());

        StShared.WriteErrorLine("par does not created", useConsole, logger);
        return null;
    }
}