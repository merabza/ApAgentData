using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using LibApAgentData.Steps;
using LibApiClientParameters;
using LibDatabaseParameters;
using LibFileParameters.Interfaces;
using LibFileParameters.Models;
using LibToolActions.BackgroundTasks;
using Microsoft.Extensions.Logging;
using SystemToolsShared;

namespace LibApAgentData.Models;

public sealed class ApAgentParameters : IParametersWithFileStorages, IParametersWithDatabaseServerConnections,
    IParametersWithApiClients, IParametersWithSmartSchemas, IParametersWithArchivers, IParametersWithExcludeSets
{
    public const string DefaultUploadFileTempExtension = ".up!";
    public const string DefaultDownloadFileTempExtension = ".down!";
    public const string DefaultArchivingFileTempExtension = ".go!";
    public const string DefaultDateMask = "yyyyMMddHHmmss";


    public string? LogFolder { get; set; }
    public string? WorkFolder { get; set; }
    public string? ProcLogFilesFolder { get; set; }
    public string? ApAgentParametersFileNameForLocalReServer { get; set; }
    public string? UploadFileTempExtension { get; set; } //.up!
    public string? DownloadFileTempExtension { get; set; } //.down!
    public string? ArchivingFileTempExtension { get; set; } //.go!
    public string? DateMask { get; set; } //.go!
    public Dictionary<string, ReplacePairsSet> ReplacePairsSets { get; set; } = [];
    public Dictionary<string, JobSchedule> JobSchedules { get; set; } = [];
    public Dictionary<string, DatabaseBackupStep> DatabaseBackupSteps { get; set; } = [];
    public Dictionary<string, MultiDatabaseProcessStep> MultiDatabaseProcessSteps { get; set; } = [];
    public Dictionary<string, RunProgramStep> RunProgramSteps { get; set; } = [];
    public Dictionary<string, ExecuteSqlCommandStep> ExecuteSqlCommandSteps { get; set; } = [];
    public Dictionary<string, FilesBackupStep> FilesBackupSteps { get; set; } = [];
    public Dictionary<string, FilesSyncStep> FilesSyncSteps { get; set; } = [];
    public Dictionary<string, FilesMoveStep> FilesMoveSteps { get; set; } = [];
    public Dictionary<string, UnZipOnPlaceStep> UnZipOnPlaceSteps { get; set; } = [];
    public List<JobStepBySchedule> JobsBySchedules { get; set; } = [];
    public Dictionary<string, ApiClientSettings> ApiClients { get; set; } = [];
    public Dictionary<string, ArchiverData> Archivers { get; set; } = [];
    public Dictionary<string, DatabaseServerConnectionData> DatabaseServerConnections { get; set; } = [];
    public Dictionary<string, ExcludeSet> ExcludeSets { get; set; } = [];
    public Dictionary<string, FileStorageData> FileStorages { get; set; } = [];

    public bool CheckBeforeSave()
    {
        var steps = GetSteps();
        var jobStepNames = JobsBySchedules.Select(s => s.JobStepName).ToList();

        var missingJobStepNames = jobStepNames.Except(steps.Keys).ToList();

        var jb = missingJobStepNames
            .Select(missingJobStepName => JobsBySchedules.Where(x => x.JobStepName == missingJobStepName))
            .SelectMany(jbs => jbs).ToList();
        foreach (var j in jb) JobsBySchedules.Remove(j);

        return true;
    }

    public Dictionary<string, SmartSchema> SmartSchemas { get; set; } = [];

    public string? CountLocalPath(string? currentPath, string? parametersFileName, string defaultFolderName)
    {
        if (!string.IsNullOrWhiteSpace(currentPath))
            return currentPath;
        var pf = string.IsNullOrWhiteSpace(parametersFileName) ? null : new FileInfo(parametersFileName);
        var workFolder = WorkFolder ?? pf?.Directory?.FullName;
        var workFolderCandidate = workFolder is null ? null : Path.Combine(workFolder, defaultFolderName);
        return workFolderCandidate;
    }

    public string GetUploadFileTempExtension()
    {
        return UploadFileTempExtension ?? DefaultUploadFileTempExtension;
    }

    public string GetDownloadFileTempExtension()
    {
        return DownloadFileTempExtension ?? DefaultDownloadFileTempExtension;
    }

    public string GetArchivingFileTempExtension()
    {
        return ArchivingFileTempExtension ?? DefaultArchivingFileTempExtension;
    }

    public string GetDateMask()
    {
        return DateMask ?? DefaultDateMask;
    }

    public Dictionary<string, JobStep> GetSteps()
    {
        Dictionary<string, JobStep> steps = new();

        foreach (var kvp in DatabaseBackupSteps)
            steps.Add(kvp.Key, kvp.Value);

        foreach (var kvp in MultiDatabaseProcessSteps)
            steps.Add(kvp.Key, kvp.Value);

        foreach (var kvp in RunProgramSteps)
            steps.Add(kvp.Key, kvp.Value);

        foreach (var kvp in ExecuteSqlCommandSteps)
            steps.Add(kvp.Key, kvp.Value);

        foreach (var kvp in FilesBackupSteps)
            steps.Add(kvp.Key, kvp.Value);

        foreach (var kvp in FilesSyncSteps)
            steps.Add(kvp.Key, kvp.Value);

        foreach (var kvp in FilesMoveSteps)
            steps.Add(kvp.Key, kvp.Value);

        foreach (var kvp in UnZipOnPlaceSteps)
            steps.Add(kvp.Key, kvp.Value);

        return steps;
    }

    public void ClearAll()
    {
        ClearSteps();
        Archivers.Clear();
        JobSchedules.Clear();
        SmartSchemas.Clear();
        ExcludeSets.Clear();
        FileStorages.Clear();
        DatabaseServerConnections.Clear();
    }

    public void ClearSteps()
    {
        JobsBySchedules.Clear();
        FilesBackupSteps.Clear();
        ExecuteSqlCommandSteps.Clear();
        RunProgramSteps.Clear();
        MultiDatabaseProcessSteps.Clear();
        DatabaseBackupSteps.Clear();
    }

    public Dictionary<string, JobSchedule> GetNotStartUpJobSchedules()
    {
        var steps = GetSteps();

        //&& s.jsFreqType != EFreqTypes.WhenCpuIdle 
        return JobSchedules
            .Where(w => w.Value.ScheduleType != EScheduleType.AtStart && w.Value.Enabled &&
                        JobsBySchedules.Any(j => steps.ContainsKey(j.JobStepName) && steps[j.JobStepName].Enabled))
            .ToDictionary(k => k.Key, v => v.Value);
        //&& s.JobsRow != null && s.JobsRow.jobEnabled && s.JobsRow.GetJobStepsRows().Any(j => j.jsEnabled));
    }

    public Dictionary<string, JobSchedule> GetStartUpJobSchedules(bool byTime,
        Dictionary<string, DateTime> nextRunDatesByScheduleNames)
    {
        var nowDateTime = DateTime.Now;
        return JobSchedules.Where(delegate(KeyValuePair<string, JobSchedule> w)
        {
            var nextRunDate = nextRunDatesByScheduleNames.GetValueOrDefault(w.Key);
            return (byTime
                ? w.Value.ScheduleType != EScheduleType.AtStart && nextRunDate != default && nextRunDate <= nowDateTime
                : w.Value.ScheduleType == EScheduleType.AtStart) && w.Value.Enabled;
        }).ToDictionary(k => k.Key, v => v.Value);
    }

    public bool RunAllSteps(ILogger logger, IHttpClientFactory httpClientFactory, bool useConsole, string scheduleName,
        IProcesses processes, string procLogFilesFolder)
    {
        if (!JobSchedules.ContainsKey(scheduleName))
            StShared.WriteErrorLine($"Schedules with name {scheduleName} not found", true, logger);

        //თუ აქ მოვედით შედულეს ბარიერი გავლილია, ან პირდაპირ არის მოთხოვნილი ამ შედულეს შესაბამისი ჯობების გაშევბა
        //შედულეს ბარიერის რეალიზება უნდა მოხდეს ბექპროცესის ტაიმერში
        var steps = GetSteps();
        var jobStepNames = JobsBySchedules.Where(s => s.ScheduleName == scheduleName).OrderBy(o => o.SequentialNumber)
            .Select(s => s.JobStepName).ToList();

        var missingJobStepNames = jobStepNames.Except(steps.Keys).ToList();

        if (missingJobStepNames.Count <= 0)
        {
            // ReSharper disable once using
            using var processManager = processes.GetNewProcessManager();
            try
            {
                foreach (var stepToolAction in jobStepNames.Select(name =>
                             steps[name].GetToolAction(logger, httpClientFactory, useConsole, processManager, this,
                                 procLogFilesFolder)))
                    if (stepToolAction is not null)
                        processManager.Run(stepToolAction);
                return true;
            }
            catch (OperationCanceledException e)
            {
                logger.LogError(e, "OperationCanceledException");
            }
            catch (Exception e)
            {
                logger.LogError(e, "Exception");
            }

            return false;
        }

        foreach (var stepName in missingJobStepNames)
            StShared.WriteErrorLine($"Step with name {stepName} not found", true, logger);
        return false;
    }
}