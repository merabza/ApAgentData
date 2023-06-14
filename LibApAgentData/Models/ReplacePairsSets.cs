using System.Collections.Generic;

namespace LibApAgentData.Models;

public sealed class ReplacePairsSets
{
    private readonly Dictionary<string, ReplacePairsSet> _replacePairsSet;

    public ReplacePairsSets(Dictionary<string, ReplacePairsSet> replacePairsSet)
    {
        _replacePairsSet = replacePairsSet;
    }

    public ReplacePairsSet? GetReplacePairsSetByKey(string key)
    {
        return !string.IsNullOrWhiteSpace(key) && _replacePairsSet.ContainsKey(key) ? _replacePairsSet[key] : null;
    }
}