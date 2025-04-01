namespace CodeRace.src;

internal class Core(
    Dictionary<string, (string, double)>? dict,
    string textPath)
{
    internal void Run()
    {
        var text = File.ReadAllText(textPath);
        Console.WriteLine($"待测文本共{text.Length}字。计算最优编码...");
        var (code, timeCost) = GetBestCode(text);
        Console.WriteLine($"计算完成。分析编码...");
        var report = UsageAnalyzer.Analyze(text, (code, timeCost));
        Console.WriteLine($"分析完成。输出报告...");
        OutputReport(report);
        Console.WriteLine($"报告已保存至待测文本所在目录。计算结束。");
    }

    private (string, double) GetBestCode(string text)
    {
        if (dict == null) throw new ArgumentException("字典不存在！");
        var routes = Enumerable.Range(0, text.Length + 1)
            .Select(_ => new HashSet<(string code, double timeCost)>())
            .ToArray()
            .AsSpan(); // 索引为下一个词的起始位置
        _ = routes[0].Add(("", 0));
        var dictWords = dict.Keys.ToArray().AsSpan();
        for (int i = 0; i < text.Length; i++)
        {
            var (bestRoute, timeCost) = routes[i].MinBy(x => x.timeCost);
            var count = 0;
            foreach (var word in dictWords)
            {
                if (text[i..].StartsWith(word))
                {
                    var (newCode, newTimeCost) = dict[word];
                    var (sumRoute, sumTimeCost) = Encoder.Join(
                        bestRoute, timeCost, newCode, newTimeCost);
                    _ = routes[i + word.Length].Add((sumRoute, sumTimeCost));
                    count++;
                }
            }
            if (count == 0)
            {
                var newCode = text[i..(i + 1)];
                var (sumRoute, sumTimeCost) = Encoder.Join(
                        bestRoute, timeCost, newCode, 0);
                _ = routes[i + 1].Add((sumRoute, sumTimeCost));
            }
            routes[i].Clear();
            Console.Write($"\r已计算至第{i + 1}字。");
        }
        return routes[text.Length].MinBy(x => x.timeCost);
    }

    private void OutputReport(string[] report)
    {

    }
}
