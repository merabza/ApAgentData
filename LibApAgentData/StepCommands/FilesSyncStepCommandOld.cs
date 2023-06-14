//using System;
//using System.Collections.Generic;
//using System.Linq;
//using SystemToolsShared;
//using CliParametersEdit.Fabrics;
//using CliParametersExcludeSetsEdit.Models;
//using CliToolsData.Models;
//using ConnectTools;
//using FileManagersMain;
//using LibApAgentData.Steps;
//using LibToolActions.BackgroundTasks;
//using Microsoft.Extensions.Logging;

//namespace LibApAgentData.StepCommands
//{

//    public sealed class FilesSyncStepCommandOld : ProcessesToolAction
//    {
//        private readonly FilesSyncStep _fileSyncStep;
//        private readonly string _uploadTempExtension;
//        private readonly ExcludeSets _excludeSets;
//        private readonly FileStorages _fileStorages;

//        private FileManager _uploadFileManager;
//        private FileManager _localFileManager;
//        private ExcludeSet _excludeSet;

//        public FilesSyncStepCommandOld(ILogger logger, ProcessManager processManager, FilesSyncStep fileSyncStep,
//            string uploadTempExtension, ExcludeSets excludeSets, FileStorages fileStorages) : base(logger,
//            processManager, "Files Sync", fileSyncStep.ProcLineId)
//        {
//            _fileSyncStep = fileSyncStep;
//            _uploadTempExtension = uploadTempExtension;
//            _excludeSets = excludeSets;
//            _fileStorages = fileStorages;
//            _localFileManager = null;
//        }

//        protected override bool RunAction()
//        {

//            Logger.LogInformation("Checking parameters...");

//            //if (string.IsNullOrWhiteSpace(_fileSyncStep.Prefix))
//            //{
//            //    StShared.WriteErrorLine("Prefix not specified for Files Backup step");
//            //    return false;
//            //}

//            //if (_fileSyncStep.BackupFolderPaths == null)
//            //{
//            //    StShared.WriteErrorLine("Backup Folder Paths not specified for Files Backup step");
//            //    return false;
//            //}

//            ExcludeSet excludeSet = _excludeSets.GetExcludeSetByKey(_fileSyncStep.ExcludeSet);
//            FileStorageData sourceFileStorage = _fileStorages.GetFileStorageDataByKey(_fileSyncStep.SourceFileStorageName);
//            FileStorageData destinationFileStorage = _fileStorages.GetFileStorageDataByKey(_fileSyncStep.DestinationFileStorageName);

//            if ( sourceFileStorage == null )
//            {
//                StShared.WriteErrorLine("Source File Storage not specified");
//                return false;
//            }

//            if ( destinationFileStorage == null )
//            {
//                StShared.WriteErrorLine("Destination File Storage not specified");
//                return false;
//            }


//            return ExecuteSync(sourceFileStorage, destinationFileStorage, excludeSet);

//        }

//        private bool ExecuteSync(FileStorageData sourceFileStorage, FileStorageData destinationFileStorage, ExcludeSet excludeSet)
//        {
//            //ფოლდერის სახელი, სადაც უნდა აიტვირთოს (დასინქრონიზდეს) ფაილები
//            string uploadFolderName = prefix + folderKey;

//            //შევქმნათ ასატვირთი ფაილ მენეჯერი
//            _uploadFileManager = FileManagersFabricExt.CreateFileManager(Logger, folderPath, uploadFileStorage);

//            //შევქმნათ ლოკალური ფაილ მენეჯერი
//            _localFileManager = FileManagersFabric.CreateFileManager(Logger, folderPath);

//            //შევამოწმოთ არსებობს თუ არა ასატვირთი ფოლდერი და თუ არ არსებობს, შევქმნათ.
//            if (!_uploadFileManager.CreateFolderIfNotExists(uploadFolderName))
//                return false;

//            //_excludes = excludeSet.FolderFileMasks.ToArray();
//            _excludeSet = excludeSet;

//            return ProcessFolder(uploadFolderName);

//        }

//        private bool ProcessFolder(string afterRootPath, string localAfterRootPath = null)
//        {
//            Console.WriteLine($"Process Folder {afterRootPath}");

//            bool isLess = false;
//            List<string> localFolderNames = _localFileManager.GetFolderNames(localAfterRootPath, null).OrderBy(o => o).ToList();
//            List<string> folderNames = _uploadFileManager.GetFolderNames(afterRootPath, null).OrderBy(o => o).ToList();
//            int i = 0;
//            int j = 0;
//            //ერთ-ერთ სიაში მაინც უნდა იყოს დარჩენილი ჩანაწერი
//            while (i < localFolderNames.Count || j < folderNames.Count)
//            {
//                //დავადგინოთ ორივე სიაში ერთდროულად არის თუ არა მიმდინარე ჩანაწერი
//                bool bHRec = i < localFolderNames.Count && j < folderNames.Count;
//                //თუ ორივეში არის ჩანაწერი და თან ერთმანეთს ემთხვევა სახელებით
//                if (bHRec && localFolderNames[i] == folderNames[j])
//                {
//                    string localFolderAfterRootFullName = _localFileManager.PathCombine(localAfterRootPath, localFolderNames[i]);
//                    string folderAfterRootFullName = _uploadFileManager.PathCombine(afterRootPath, folderNames[j]);

//                    //შევადაროთ შიგთავსები
//                    if (!ProcessFolder(folderAfterRootFullName, localFolderAfterRootFullName))
//                        return false;
//                    i++;
//                    j++;
//                }
//                else
//                {//უკვე დადგენილია, რომ ფოლდერების სახელები განსხვავდება
//                    if (bHRec)//თუ ორივე სიაში არის მიმდინარე ჩანაწერი, მაშინ დავადგინოთ, რომელია ნაკლები
//                        isLess = string.CompareOrdinal(localFolderNames[i], folderNames[j]) < 0;
//                    //თუ ლოკალურში არის ჩანაწერი და მიზანში არა, ან ორივეში არის და წყაროს კოდი ნაკლებია
//                    if (i < localFolderNames.Count && j >= folderNames.Count || bHRec && isLess)
//                    {//შეიქმნას ახალი ფოლდერი მაღლა
//                        Console.WriteLine($"Create Folder {afterRootPath}/{localFolderNames[i]}");
//                        if (!_uploadFileManager.CreateDirectory(afterRootPath, localFolderNames[i]))
//                            return false;
//                        string localFolderAfterRootFullName = _localFileManager.PathCombine(localAfterRootPath, localFolderNames[i]);
//                        string folderAfterRootFullName = _uploadFileManager.PathCombine(afterRootPath, localFolderNames[i]);
//                        if (!ProcessFolder(folderAfterRootFullName, localFolderAfterRootFullName))
//                            return false;
//                        i++;
//                    }
//                    else if (j < folderNames.Count && i >= localFolderNames.Count || bHRec)
//                    {
//                        Console.WriteLine($"Remove Folder {afterRootPath}/{folderNames[j]}");
//                        if (!_uploadFileManager.DeleteDirectory(afterRootPath, folderNames[j], true))
//                            return false;
//                        j++;
//                    }

//                }

//            }

//            return ProcessFiles(afterRootPath, localAfterRootPath);

//        }

//        private bool ProcessFiles(string afterRootPath, string localAfterRootPath = null)
//        {


//            bool isLess = false;
//            List<MyFileInfo> localFileNames = _localFileManager.GetFilesWithInfo(localAfterRootPath, null)
//                .OrderBy(o => o.FileName).ToList();
//            List<MyFileInfo> fileNames = _uploadFileManager.GetFilesWithInfo(afterRootPath, null)
//                .OrderBy(o => o.FileName).ToList();
//            int i = 0;
//            int j = 0;
//            //ერთ-ერთ სიაში მაინც უნდა იყოს დარჩენილი ჩანაწერი
//            while (i < localFileNames.Count || j < fileNames.Count)
//            {
//                if (i < localFileNames.Count)
//                {
//                    //თუ ლოკალური ფაილის სრული სახელი გამოსარიცხებშია
//                    string localFileAfterRootFullName =
//                        _localFileManager.PathCombine(localAfterRootPath, localFileNames[i].FileName);
//                    if (_excludeSet.NeedExclude(localFileAfterRootFullName))
//                    {
//                        i++;
//                        continue;
//                    }
//                }
//                if ( j < fileNames.Count)
//                {
//                    //თუ ატვირთული ფაილის სრული სახელი გამოსარიცხებშია
//                    string fileAfterRootFullName = _uploadFileManager.PathCombine(afterRootPath, fileNames[j].FileName);
//                    if (_excludeSet.NeedExclude(fileAfterRootFullName))
//                    {
//                        j++;
//                        continue;
//                    }
//                }
//                //დავადგინოთ ორივე სიაში ერთდროულად არის თუ არა მიმდინარე ჩანაწერი
//                bool bHRec = i < localFileNames.Count && j < fileNames.Count;
//                //თუ ორივეში არის ჩანაწერი და თან ერთმანეთს ემთხვევა სახელებით
//                if (bHRec && localFileNames[i].FileName == fileNames[j].FileName)
//                {
//                    //შევადაროთ მხოლოდ სიგრძეები, რადგან სხვა ინფორმაცია ან არასანდოა, ან დიდ დროს მოითხოვს
//                    if (localFileNames[i].FileLength != fileNames[j].FileLength)
//                    {
//                        Console.WriteLine($"Delete File {afterRootPath}/{fileNames[j].FileName}");
//                        if (!_uploadFileManager.DeleteFile(afterRootPath, fileNames[j].FileName))
//                            return false;
//                        Console.WriteLine(
//                            $"Upload From {localAfterRootPath} File {fileNames[j].FileName} to {afterRootPath}");
//                        if (!_uploadFileManager.UploadFile(localAfterRootPath, fileNames[j].FileName, afterRootPath,
//                            fileNames[j].FileName, _uploadTempExtension))
//                            return false;
//                    }
//                    i++;
//                    j++;
//                }
//                else
//                {//უკვე დადგენილია, რომ ფოლდერების სახელები განსხვავდება
//                    if (bHRec) //თუ ორივე სიაში არის მიმდინარე ჩანაწერი, მაშინ დავადგინოთ, რომელია ნაკლები
//                        isLess = string.CompareOrdinal(localFileNames[i].FileName, fileNames[j].FileName) < 0;
//                    //თუ ლოკალურში არის ჩანაწერი და მიზანში არა, ან ორივეში არის და წყაროს კოდი ნაკლებია
//                    if (i < localFileNames.Count && j >= fileNames.Count || bHRec && isLess)
//                    {//შეიქმნას ახალი ფოლდერი მაღლა
//                        //string localFileFullName =
//                        //    _localFileManager.PathCombine(localAfterRootPath, localFileNames[i].FileName);
//                        Console.WriteLine(
//                            $"Upload from {localAfterRootPath} File {localFileNames[i].FileName} to {afterRootPath}");
//                        //if (!_uploadFileManager.UploadFile(afterRootPath, localFileNames[i].FileName))
//                        if (!_uploadFileManager.UploadFile(localAfterRootPath, localFileNames[i].FileName,
//                            afterRootPath, localFileNames[i].FileName, _uploadTempExtension))
//                            return false;
//                        i++;
//                    }
//                    else if (j < fileNames.Count && i >= localFileNames.Count || bHRec)
//                    {
//                        Console.WriteLine($"Delete File {afterRootPath}/{fileNames[j].FileName}");
//                        if (!_uploadFileManager.DeleteFile(afterRootPath, fileNames[j].FileName))
//                            return false;
//                        j++;
//                    }

//                }

//            }

//            return true;

//        }

//    }

//}

