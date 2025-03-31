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
        Console.WriteLine($"计算完成，平均码长为{code.Length / text.Length}。分析编码...");
        var report = UsageAnalyzer.Analyze(text, (code, timeCost));
        Console.WriteLine($"分析完成。输出报告...");
        OutputReport(report);
        Console.WriteLine($"报告已保存至待测文本所在目录。计算结束。");
    }

    private (string, double) GetBestCode(string text)
    {

    }

    private void OutputReport(string[] report)
    {

    }
}
