using System.Collections.Generic;

namespace ApAgentData.LibApAgentData.Models;

public sealed class FileListModel
{
    public List<FileModel> Files { get; } = [];
    public Dictionary<string, DuplicateFilesStorage> DuplicateFilesStorage { get; } = new();
}
