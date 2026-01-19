using System.Collections.Generic;

namespace ApAgentData.LibApAgentData.Models;

public sealed class DuplicateFilesModel
{
    public List<FileModel> Files { get; set; } = [];
}
