namespace CodeRace.src;

internal class Core(
    Dictionary<string, (string, double)>? dict,
    string textPath)
{
    internal void Run()
    {
        var text = File.ReadAllText(textPath);
        Console.WriteLine($"待测文本共{text.Length}字。计算最优编码...");
        var (bestRoute, timeCost) = GetBestCode(text);
        Console.WriteLine("计算完成。分析编码...");
        var report = UsageAnalyzer.Analyze(text.Length, bestRoute, timeCost);
        Console.WriteLine("分析完成。输出报告...");
        if (!Report2File(report))
            Report2Console(report);
    }

    private (string, double) GetBestCode(string text)
    {
        if (dict == null)
            throw new ArgumentException("字典不存在！");
        var routes = Enumerable.Range(0, text.Length + 1)
            .Select(_ => new HashSet<(string code, double timeCost)>())
            .ToArray()
            .AsSpan(); // 索引为下一个词的起始位置
        _ = routes[0].Add(("", 0));
        var dictWords = dict.Keys.ToArray().AsSpan();
        for (int i = 0; i < text.Length; i++)
        {
            var (bestRoute, timeCost) = routes[i].MinBy(x => x.timeCost);
            var matched = false;
            foreach (var word in dictWords)
            {
                if (text[i..].StartsWith(word))
                {
                    var (newCode, newTimeCost) = dict[word];
                    var (sumRoute, sumTimeCost) = Encoder.Join(
                        bestRoute, timeCost, newCode, newTimeCost);
                    _ = routes[i + word.Length].Add((sumRoute, sumTimeCost));
                    matched = true;
                }
            }
            if (!matched)
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

    private bool Report2File(string[] report)
    {
        try
        {
            var dir = Path.GetDirectoryName(textPath)
                ?? throw new ArgumentException("无法获取待测文本所在目录！");
            var textFileName = Path.GetFileNameWithoutExtension(textPath);
            var reportPath = Path.Combine(dir, $"{textFileName}_code_report.txt");
            for (int i = 2; File.Exists(reportPath); i++)
                reportPath = Path.Combine(dir, $"{textFileName}_code_report_{i}.txt");
            File.WriteAllLines(reportPath, report);
            Console.WriteLine($"报告已保存至：{reportPath}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"报告保存失败：{ex.Message}");
            return false;
        }
    }

    private static void Report2Console(string[] report)
    {
        Console.WriteLine("将报告直接输出到控制台...");
        foreach (var line in report)
            Console.WriteLine(line);
        Console.WriteLine("报告输出完毕。");
    }
}
