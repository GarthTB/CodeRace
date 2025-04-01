namespace CodeRace.src;

internal static class CodeAnalyzer
{
    internal static string[] Layout { get; set; } = [];

    internal static string[] Analyze(
        int textLength, string bestRoute, double timeCost)
    {
        uint numCount = 0;
        uint upCount = 0;
        uint midCount = 0;
        uint downCount = 0;
        uint spaceCount = 0; // 空格排
        uint left5Count = 0;
        uint left4Count = 0;
        uint left3Count = 0;
        uint left2Count = 0;
        uint right2Count = 0;
        uint right3Count = 0;
        uint right4Count = 0;
        uint right5Count = 0;
        uint thumbCount = 0; // 拇指键
        uint doubleClickCount = 0;
        uint tripleClickCount = 0;
        uint quadrupleClickCount = 0;
        _ = Parallel.For(0, bestRoute.Length, i =>
        {
            var c = bestRoute[i];
            if (Layout[0].Contains(c)) _ = Interlocked.Increment(ref numCount);
            if (Layout[1].Contains(c)) _ = Interlocked.Increment(ref upCount);
            if (Layout[2].Contains(c)) _ = Interlocked.Increment(ref midCount);
            if (Layout[3].Contains(c)) _ = Interlocked.Increment(ref downCount);
            if (Layout[4].Contains(c)) _ = Interlocked.Increment(ref spaceCount);
            if (Layout[5].Contains(c)) _ = Interlocked.Increment(ref left5Count);
            if (Layout[6].Contains(c)) _ = Interlocked.Increment(ref left4Count);
            if (Layout[7].Contains(c)) _ = Interlocked.Increment(ref left3Count);
            if (Layout[8].Contains(c)) _ = Interlocked.Increment(ref left2Count);
            if (Layout[9].Contains(c)) _ = Interlocked.Increment(ref right2Count);
            if (Layout[10].Contains(c)) _ = Interlocked.Increment(ref right3Count);
            if (Layout[11].Contains(c)) _ = Interlocked.Increment(ref right4Count);
            if (Layout[12].Contains(c)) _ = Interlocked.Increment(ref right5Count);
            if (Layout[13].Contains(c)) _ = Interlocked.Increment(ref thumbCount);
            if (c == bestRoute[i - 1]) _ = Interlocked.Increment(ref doubleClickCount);
            if (c == bestRoute[i - 1] && c == bestRoute[i - 2])
                _ = Interlocked.Increment(ref tripleClickCount);
            if (c == bestRoute[i - 1] && c == bestRoute[i - 2] && c == bestRoute[i - 3])
                _ = Interlocked.Increment(ref quadrupleClickCount);
        });
        var leftSum = left5Count + left4Count + left3Count + left2Count;
        var rightSum = right5Count + right4Count + right3Count + right2Count;

        return
        [
            bestRoute,
            "---以上为最优编码路径，以下为分析---",
            $"字数\t{textLength}",
            $"码数\t{bestRoute.Length}",
            $"码长\t{bestRoute.Length / (double)textLength:F4}",
            $"当量\t{timeCost:F1}",
            $"字均当量\t{timeCost / textLength:F4}",
            $"码均当量\t{timeCost / bestRoute.Length:F4}",
            $"数排\t{numCount}\t{100.0 * numCount / bestRoute.Length:F2}%",
            $"上排\t{upCount}\t{100.0 * upCount / bestRoute.Length:F2}%",
            $"中排\t{midCount}\t{100.0 * midCount / bestRoute.Length:F2}%",
            $"下排\t{downCount}\t{100.0 * downCount / bestRoute.Length:F2}%",
            $"空格排\t{spaceCount}\t{100.0 * spaceCount / bestRoute.Length:F2}%",
            $"总左手\t{leftSum}\t{100.0 * leftSum / bestRoute.Length:F2}%",
            $"总右手\t{rightSum}\t{100.0 * rightSum / bestRoute.Length:F2}%",
            $"偏倚率\t{100.0 * (leftSum - rightSum) / (leftSum + rightSum):F2}%",
            $"左小指\t{left5Count}\t{100.0 * left5Count / bestRoute.Length:F2}%",
            $"左无名\t{left4Count}\t{100.0 * left4Count / bestRoute.Length:F2}%",
            $"左中指\t{left3Count}\t{100.0 * left3Count / bestRoute.Length:F2}%",
            $"左食指\t{left2Count}\t{100.0 * left2Count / bestRoute.Length:F2}%",
            $"右食指\t{right2Count}\t{100.0 * right2Count / bestRoute.Length:F2}%",
            $"右中指\t{right3Count}\t{100.0 * right3Count / bestRoute.Length:F2}%",
            $"右无名\t{right4Count}\t{100.0 * right4Count / bestRoute.Length:F2}%",
            $"右小指\t{right5Count}\t{100.0 * right5Count / bestRoute.Length:F2}%",
            $"拇指键\t{thumbCount}\t{100.0 * thumbCount / bestRoute.Length:F2}%",
            $"两连击\t{doubleClickCount}\t{100.0 * doubleClickCount / bestRoute.Length:F2}%",
            $"三连击\t{tripleClickCount}\t{100.0 * tripleClickCount / bestRoute.Length:F2}%",
            $"四连击\t{quadrupleClickCount}\t{100.0 * quadrupleClickCount / bestRoute.Length:F2}%"
        ];
    }
}
