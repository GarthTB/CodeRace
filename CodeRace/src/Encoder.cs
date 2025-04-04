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

    internal static (string, double) Join(
        string head,
        double oldTimeCost,
        string tail,
        double newTimeCost)
    {
        var a = "abcdefghijklmnopqrstuvwxyz"; // 字母码元
        var n = "1234567890"; // 数字（选重）

        return JoinMethodIndex switch
        {
            0 => Punctuations(),
            1 => ($"{head}{tail}", SumTimeCost(head, "", tail)),
            2 => JianDao(),
            _ => throw new ArgumentException("编码连接方法代号错误！")
        };

        double SumTimeCost(string _head, string middle, string _tail)
            => _head.Length > 0
                ? oldTimeCost
                  + newTimeCost
                  + GetTimeCost($"{_head[^1]}{middle}{_tail[0]}")
                : newTimeCost;

        (string, double) Punctuations()
            => (a.Contains(tail[0]) || n.Contains(tail[0])) // 新码以非标点开头，
                && head.Length > 0 && a.Contains(head[^1]) // 且前码为字母：加空格
                ? ($"{head} {tail}", SumTimeCost(head, " ", tail))
                : ($"{head}{tail}", SumTimeCost(head, "", tail));

        (string, double) JianDao()
        {
            var x = "aiouv"; // 形码码元
            var y = "bcdefghjklmnpqrstwxyz"; // 音码码元
            if (tail.Length < 4 && y.Contains(tail[^1]))
            {   // 新码不足4码，且以音码结尾：后补空格
                newTimeCost += GetTimeCost($"{tail[^1]} ");
                tail = $"{tail} ";
            }
            if (head.Length == 0) // 没有上文：直接返回
                return (tail, newTimeCost);
            if (!(a.Contains(tail[0]) || n.Contains(tail[0])) && head[^1] == ' ')
            {   // 新码以标点开头，且前为空格：去掉空格
                oldTimeCost -= GetTimeCost($"{head[^1]} ");
                head = head[..^1];
            }
            if ((x.Contains(tail[0]) || n.Contains(tail[0])) && a.Contains(head[^1]))
            {   // 新码以形码或数字开头，且前为字母：前加空格
                newTimeCost += GetTimeCost($" {tail[0]}");
                tail = $" {tail}";
            }
            return ($"{head}{tail}", SumTimeCost(head, "", tail));
        }
    }
}
