namespace YamanCam.Web.Services;

public static class AccountPlanCodeHelper
{
    // Noktasiz kodlarda TDHP seviye uzunluklari: 3, 5, 7, 9, 12, 14
    private static readonly int[] LevelLengths = [3, 5, 7, 9, 12, 14];

    public static string NormalizeCode(string accountCode) => accountCode.Trim();

    public static int GetLevelNo(string accountCode)
    {
        var code = NormalizeCode(accountCode);
        if (string.IsNullOrEmpty(code))
        {
            return 1;
        }

        if (code.Contains('.'))
        {
            return code.Split('.', StringSplitOptions.RemoveEmptyEntries).Length;
        }

        var length = code.Length;
        for (var i = LevelLengths.Length - 1; i >= 0; i--)
        {
            if (length > LevelLengths[i])
            {
                return i + 2;
            }

            if (length == LevelLengths[i])
            {
                return i + 1;
            }
        }

        return 1;
    }

    public static string? GetParentAccountCode(string accountCode)
    {
        var code = NormalizeCode(accountCode);
        if (string.IsNullOrEmpty(code))
        {
            return null;
        }

        if (code.Contains('.'))
        {
            var lastDot = code.LastIndexOf('.');
            return lastDot > 0 ? code[..lastDot] : null;
        }

        if (code.Length <= LevelLengths[0])
        {
            return null;
        }

        for (var i = LevelLengths.Length - 1; i >= 0; i--)
        {
            if (code.Length > LevelLengths[i])
            {
                return code[..LevelLengths[i]];
            }
        }

        return null;
    }
}
