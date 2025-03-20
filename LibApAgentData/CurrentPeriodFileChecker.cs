using System;
using System.Linq;
using FileManagersMain;
using SystemToolsShared;

namespace LibApAgentData;

public sealed class CurrentPeriodFileChecker
{
    private readonly string _dateMask;
    private readonly TimeSpan _holeEndTime;
    private readonly TimeSpan _holeStartTime;
    private readonly EPeriodType _periodType;
    private readonly string _prefix;
    private readonly DateTime _startAt;
    private readonly string _suffix;
    private readonly FileManager _workFileManager;

    // ReSharper disable once ConvertToPrimaryConstructor
    public CurrentPeriodFileChecker(EPeriodType periodType, DateTime startAt, TimeSpan holeStartTime,
        TimeSpan holeEndTime, string prefix, string dateMask, string suffix, FileManager workFileManager)
    {
        _periodType = periodType;
        _startAt = startAt;
        _holeStartTime = holeStartTime;
        _holeEndTime = holeEndTime;
        _prefix = prefix;
        _dateMask = dateMask;
        _suffix = suffix;
        _workFileManager = workFileManager;
    }

    public bool HaveCurrentPeriodFile()
    {
        var nowDateTime = DateTime.Now;
        var endTime = DateTime.Today + _holeEndTime;

        if (endTime < nowDateTime)
            return true;

        var start = _startAt + _holeStartTime;
        var currentPeriodStart = start.DateAdd(_periodType, nowDateTime.DateDiff(_periodType, start));
        var currentPeriodEnd = currentPeriodStart.DateAdd(_periodType, 1);
        var files = _workFileManager.GetFilesByMask(_prefix, _dateMask, _suffix);
        return files.Any(s => s.FileDateTime >= currentPeriodStart && s.FileDateTime < currentPeriodEnd);
    }
}