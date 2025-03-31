namespace CodeRace.src;

internal static class Encoder
{
    internal static Dictionary<string, double> TimeCostMap { get; set; } = [];

    internal static int JoinMethodIndex { get; set; } = 0;

    internal static double GetTimeCost(string text)
    {
        var cost = 0.0;
        for (int i = 0; i < text.Length - 1; i++)
        {
            var key = text.Substring(i, 2);
            if (!TimeCostMap.TryGetValue(key, out double value))
            {
                Console.WriteLine($"找不到{key}对应的当量。已默认为1.5。");
                cost += 1.5;
            }
            else cost += value;
        }
        return cost;
    }

    internal static string Join(string head, string tail)
    {
        var a = "abcdefghijklmnopqrstuvwxyz"; // 所有码元

        return JoinMethodIndex switch
        {
            0 => Punctuations(),
            1 => $"{head}{tail}",
            2 => JianDao(),
            _ => throw new ArgumentException("编码连接方法代号错误！")
        };

        string Punctuations()
            => a.Contains(tail[0]) && a.Contains(head[^1])
                ? $"{head} {tail}"
                : $"{head}{tail}";

        string JianDao()
        {
            var x = "aiouv"; // 形码码元
            var y = "bcdefghjklmnpqrstwxyz"; // 音码码元
            if (tail.Length < 4 && y.Contains(tail[^1]))
                tail = $"{tail} "; // 新码不足4码，以音码结尾：后补空格
            if (head.Length == 0)
                return tail; // 没有上文：直接返回
            if (!a.Contains(tail[0]) && head[^1] == ' ')
                head = head[..^1]; // 新码以标点开头，前为空格：去掉空格
            if (x.Contains(tail[0]) && a.Contains(head[^1]))
                tail = $" {tail}"; // 新码以形码开头，前无标点：前加空格
            return $"{head}{tail}";
        }
    }
}
