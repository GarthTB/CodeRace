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
        var report = CodeAnalyzer.Analyze(text.Length, bestRoute, timeCost);
        Console.WriteLine("分析完成。输出报告...");
        if (!Report2File(report))
            Report2Console(report);
    }

    private (string, double) GetBestCode(string text)
    {
        if (dict == null)
            throw new ArgumentException("词库不存在！");
        RoutesBuffer routesBuffer = new( // 索引为下一个词开始位置与当前位置的字数差
            Math.Min(text.Length + 1, dict.Keys.Max(x => x.Length) + 1));
        var groupedWords = dict.Keys.GroupBy(x => x[0])
            .ToDictionary(x => x.Key, x => x.ToArray()); // 将词库中的所有词按首字符分组
        for (int i = 0; i < text.Length; i++)
        {
            if (!routesBuffer.GetBestRoute(out var bestRoute, out var timeCost))
                continue; // 有时候编码会直接跳过某些字符
            var dictWords = groupedWords.TryGetValue(text[i], out var result)
                ? result.AsSpan()
                : [];
            var matched = false;

            foreach (var word in dictWords)
                if (string.CompareOrdinal(text, i, word, 0, word.Length) == 0)
                {
                    var (newCode, newTimeCost) = dict[word];
                    routesBuffer.Add(word.Length,
                        Encoder.Join(bestRoute, timeCost, newCode, newTimeCost));
                    matched = true;
                }

            // 常见中文标点已在词库，此处为其他字符
            if (!matched)
                routesBuffer.Add(1,
                    Encoder.Join(bestRoute, timeCost, text[i..(i + 1)], 0));

            routesBuffer.Next();
            Console.Write($"\r已计算至第{i + 1}字。");
        }
        return routesBuffer.GetBestRoute(out var finalBestRoute, out var finalTimeCost)
            ? (finalBestRoute, finalTimeCost)
            : throw new ArgumentException("编码无法到达终点！");
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
