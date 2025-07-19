using System.Collections.Generic;

namespace LibApAgentData.Models;

public sealed class DuplicateFilesModel
{
    public List<FileModel> Files { get; set; } = [];
}