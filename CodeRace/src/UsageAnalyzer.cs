namespace CodeRace.src;

internal static class UsageAnalyzer
{
    internal static string[] Layout { get; set; } = [];

    internal static string[] Analyze(
        int textLength, string bestRoute, double timeCost)
    {
        List<string> report = new(16)
        {
            bestRoute,
            "---以上为最优编码路径，以下为分析---",
            $"字数\t{textLength}",
            $"码数\t{bestRoute.Length}",
            $"码长\t{bestRoute.Length / (double)textLength:F4}",
            $"当量\t{timeCost:F1}",
            $"字均当量\t{timeCost / textLength:F4}",
            $"码均当量\t{timeCost / bestRoute.Length:F4}"
        };
        AnalyzeFingers();
        AnalyzeRows();
        return [.. report];

        void AnalyzeFingers()
        {

        }

        void AnalyzeRows()
        {

        }
    }
}
