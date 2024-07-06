using System.IO;
using System.Linq;

namespace LibApAgentData;

public static class StringExtension
{
    public static string PrepareFileName(this string fileName)
    {
        const string restrictedSymbols = "<>:\"/\\|?*'«»";
        return fileName.Intersect(restrictedSymbols)
            .Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));
    }

    public static string PreparedFileNameConsideringLength(this string fileName, int fileMaxLength)
    {
        var preparedFileName = fileName.PrepareFileName().Trim();
        var extension = Path.GetExtension(preparedFileName).Trim();
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(preparedFileName).Trim();
        preparedFileName = fileNameWithoutExtension.GetNewFileNameWithMaxLength(0, extension, fileMaxLength);
        return preparedFileName;
    }

    public static string GetNewFileName(this string fileNameWithoutExtension, int i, string fileExtension)
    {
        return $"{fileNameWithoutExtension}{(i == 0 ? string.Empty : $"({i})")}{fileExtension}";
    }

    public static string GetNewFileNameWithMaxLength(this string fileNameWithoutExtension, int i,
        string fileExtension, int maxLength = 255)
    {
        var oneTry = fileNameWithoutExtension.GetNewFileName(i, fileExtension);
        var more = oneTry.Length - maxLength;
        if (more <= 0)
            return oneTry;
        var take = fileNameWithoutExtension.Length - more;
        oneTry = fileNameWithoutExtension[..take].GetNewFileName(i, fileExtension);
        return oneTry;
    }
}