namespace CodeRace.src;

/// <summary>
/// 连接器：用于连接两串编码及其用时当量
/// </summary>
/// <param name="timeMap">
/// 按键之间的用时当量：键为两个按键对应的字符，值为用时当量
/// </param>
/// <param name="methodIndex">
/// 连接方法代号：0-空格或符号，1-无间隔，2-键道顶功
/// </param>
internal class RouteConnector(
    Dictionary<string, double> timeMap,
    int methodIndex)
{
    /// <summary>
    /// 找不到用时当量的按键组合
    /// </summary>
    internal HashSet<string> InvalidKeys { get; private set; } = [];

    internal void ReportInvalidKeys(string textPath)
        => FileHelper.Report(textPath, "找不到用时当量的按键组合", [.. InvalidKeys]);

    internal double GetTime(string code)
    {
        var timeSum = 0.0;
        for (int i = 0; i < code.Length - 1; i++)
        {
            var key = code.Substring(i, 2);
            if (!timeMap.TryGetValue(key, out var time))
            {
                _ = InvalidKeys.Add(key);
                timeSum += 1.5;
            }
            else timeSum += time;
        }
        return Math.Round(timeSum, 2); // 避免浮点数精度问题
    }

    internal (string, double) Connect(
        string s1, string s2, double t1, double t2)
    {
        return s2.Length == 0
            ? throw new ArgumentException("第二个字符串不能为空")
            : methodIndex switch
            {
                0 => SpaceOrSymbol(),
                1 => NoInterval(),
                2 => JianDao(),
                _ => throw new ArgumentException("编码连接方法代号无效"),
            };

        (string, double) SpaceOrSymbol()
        => s1.Length == 0
            ? (s2, t2)
            : IsLetter(s1[^1]) && (IsLetter(s2[0]) || IsNumber(s2[0]))
                ? ($"{s1} {s2}", t1 + t2 + GetTime($"{s1[^1]} {s2[0]}"))
                : ($"{s1}{s2}", t1 + t2 + GetTime($"{s1[^1]}{s2[0]}"));

        (string, double) NoInterval()
        => s1.Length == 0
            ? (s2, t2)
            : ($"{s1}{s2}", t1 + t2 + GetTime($"{s1[^1]}{s2[0]}"));

        (string, double) JianDao()
        {
            // 音码和形码
            var x = "aiouvAIOUV";
            var y = "bcdefghjklmnpqrstwxyzBCDEFGHJKLMNPQRSTWXYZ";

            // 新码以音码结尾，且不足4码：新码后补空格
            if (y.Contains(s2[^1]) && s2.Length < 4)
            {
                s2 = $"{s2} ";
                t2 += GetTime($"{s2[^1]} ");
            }
            // 没有上文，直接返回
            if (s1.Length == 0)
                return (s2, t2);

            // 上文末尾为空格，且新码以非空格的标点开头：去掉上文末尾的空格
            if (s1[^1] == ' '
                && s2[0] != ' '
                && !IsLetter(s2[0])
                && !IsNumber(s2[0]))
            {
                if (s1.Length > 1)
                    t1 -= GetTime(s1[^2..]);
                s1 = s1[..^1];
            }
            // 上文末尾为字母，且新码以形码或数字开头：在上文末尾加空格
            else if (IsLetter(s1[^1]) && (x.Contains(s2[0]) || IsNumber(s2[0])))
            {
                s1 = $"{s1} ";
                t1 += GetTime(s1[^2..]);
            }

            return s1.Length == 0 // 上文的空格被去掉了
                ? (s2, t2)
                : ($"{s1}{s2}", t1 + t2 + GetTime($"{s1[^1]}{s2[0]}"));
        }
    }

    private static bool IsLetter(char c)
    => "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".Contains(c);

    private static bool IsNumber(char c)
    => "1234567890".Contains(c);
}
