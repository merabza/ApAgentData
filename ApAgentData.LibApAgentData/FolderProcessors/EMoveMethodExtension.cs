using System;
using ApAgentData.LibApAgentData.StepCommands;

namespace ApAgentData.LibApAgentData.FolderProcessors;

public static class EMoveMethodExtension
{
    public static string CountTempExtension(this EMoveMethod useMethod, string uploadTempExtension,
        string downloadTempExtension)
    {
        return useMethod switch
        {
            EMoveMethod.Upload => uploadTempExtension,
            EMoveMethod.Download => downloadTempExtension,
            EMoveMethod.Local => string.Empty,
            _ => throw new ArgumentOutOfRangeException(nameof(useMethod), useMethod, "Invalid EMoveMethod value.")
        };
    }
}
