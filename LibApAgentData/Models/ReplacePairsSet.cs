using System.Collections.Generic;
using System.Linq;
using LibParameters;
using SystemToolsShared;

namespace ApAgentData.LibApAgentData.Models;

public sealed class ReplacePairsSet : ItemData
{
    //ჩანაცვლებების სია
    public Dictionary<string, string> PairsDict { get; init; } = new();

    public bool NeedExclude(string name)
    {
        return PairsDict is { Count: > 0 } && PairsDict.Keys.Any(name.FitsMask);
    }
}