//using DbTools;
//using LibApAgentData.Models;

//namespace LibApAgentData.PathCounters
//{

//    public static class LocalPathCounterFabric
//    {

//        public static LocalPathCounter CreateFileBackupsLocalPathCounter(ApAgentParameters parameters,
//            string parametersFileName)
//        {
//            return new(parameters, parametersFileName, "FilesBackups");
//        }

//        public static LocalPathCounter CreateProcLogFilesPathCounter(ApAgentParameters parameters,
//            string parametersFileName)
//        {
//            return new(parameters, parametersFileName, "ProcLogFiles");
//        }

//        public static LocalPathCounter CreateDatabaseBackupsLocalPathCounter(ApAgentParameters parameters,
//            string parametersFileName, EBackupType backupType)
//        {
//            return new(parameters, parametersFileName, $"Database{backupType}Backups");
//        }

//    }

//}

