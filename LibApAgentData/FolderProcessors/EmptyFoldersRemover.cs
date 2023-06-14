using FileManagersMain;

namespace LibApAgentData.FolderProcessors;

public sealed class EmptyFoldersRemover : FolderProcessor
{
    public EmptyFoldersRemover(
        //ILogger logger, bool useConsole, 
        FileManager fileManager) : base(
        //logger, useConsole,
        "Empty Folders Remover", "Removes Empty Folders", fileManager, null, true, null, null)
    {
    }
}