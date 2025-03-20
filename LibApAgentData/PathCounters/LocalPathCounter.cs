//using System.IO;
//using LibApAgentData.Models;

//namespace LibApAgentData.PathCounters
//{
//  public sealed class LocalPathCounter
//  {
//    private readonly ApAgentParameters _parameters;
//    private readonly string _parametersFileName;
//    private readonly string _defaultFolderName;

//    public LocalPathCounter(ApAgentParameters parameters, string parametersFileName, string defaultFolderName)
//    {
//      _parameters = parameters;
//      _parametersFileName = parametersFileName;
//      _defaultFolderName = defaultFolderName;
//    }

//    public string Count(string currentPath)
//    {
//      if (!string.IsNullOrWhiteSpace(currentPath)) 
//        return currentPath;
//      FileInfo pf = new FileInfo(_parametersFileName);
//      string? workFolder = _parameters.WorkFolder ?? pf.Directory?.FullName;
//      string workFolderCandidate = workFolder == null ? string.Empty : Path.Combine(workFolder, _defaultFolderName);
//      return workFolderCandidate;
//    }

//  }
//}

