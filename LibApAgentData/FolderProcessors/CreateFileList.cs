//using System.Collections.Generic;
//using CliParametersExcludeSetsEdit.Models;
//using ConnectTools;
//using FileManagersMain;

//namespace LibApAgentData.FolderProcessors
//{

//    public sealed class CreateFileList : FolderProcessor
//    {

//        private readonly List<string> _fileList = new();

//        public List<string> FileList => _fileList;

//        public CreateFileList(FileManager fileManager, ExcludeSet excludeSet) : base("File List", "Create File List",
//            fileManager, null, true, null, excludeSet)
//        {

//        }

//        protected override bool ProcessOneFile(string afterRootPath, MyFileInfo file,
//            RecursiveParameters recursiveParameters = null)
//        {
//            _fileList.Add(FileManager.PathCombine(afterRootPath, file.FileName));
//            return true;
//        }

//    }

//}

