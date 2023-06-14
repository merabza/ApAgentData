using System.Collections.Generic;

namespace LibApAgentData.Models;

public sealed class FileListModel
{
    public List<FileModel> Files { get; } = new();
    public Dictionary<string, DuplicateFilesStorage> DuplicateFilesStorage { get; } = new();
}