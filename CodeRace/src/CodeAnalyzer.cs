namespace CodeRace.src;

internal static class CodeAnalyzer
{
    internal static string[] Layout { get; set; } = [];

    internal static string[] Analyze(
        int textLength, string bestRoute, double timeCost)
    {
        var numCount = 0;
        var upperCount = 0;
        var midCount = 0;
        var lowerCount = 0;
        var bottomCount = 0; // 底排
        var left5Count = 0;
        var left4Count = 0;
        var left3Count = 0;
        var left2Count = 0;
        var right2Count = 0;
        var right3Count = 0;
        var right4Count = 0;
        var right5Count = 0;
        var thumbCount = 0; // 拇指键
        var alternateCount = 0; // 左右互击
        var smallLeapCount = 0; // 同指小跨排
        var bigLeapCount = 0; // 同指大跨排
        var doubleCount = 0;
        var tripleCount = 0;
        var quadrupleCount = 0;
        _ = Parallel.For(0, bestRoute.Length, i =>
        {
            var c = bestRoute[i];
            if (Layout[0].Contains(c)) _ = Interlocked.Increment(ref numCount);
            if (Layout[1].Contains(c)) _ = Interlocked.Increment(ref upperCount);
            if (Layout[2].Contains(c)) _ = Interlocked.Increment(ref midCount);
            if (Layout[3].Contains(c)) _ = Interlocked.Increment(ref lowerCount);
            if (Layout[4].Contains(c)) _ = Interlocked.Increment(ref bottomCount);
            if (Layout[5].Contains(c)) _ = Interlocked.Increment(ref left5Count);
            if (Layout[6].Contains(c)) _ = Interlocked.Increment(ref left4Count);
            if (Layout[7].Contains(c)) _ = Interlocked.Increment(ref left3Count);
            if (Layout[8].Contains(c)) _ = Interlocked.Increment(ref left2Count);
            if (Layout[9].Contains(c)) _ = Interlocked.Increment(ref right2Count);
            if (Layout[10].Contains(c)) _ = Interlocked.Increment(ref right3Count);
            if (Layout[11].Contains(c)) _ = Interlocked.Increment(ref right4Count);
            if (Layout[12].Contains(c)) _ = Interlocked.Increment(ref right5Count);
            if (Layout[13].Contains(c)) _ = Interlocked.Increment(ref thumbCount);
            if (i > 0 && Alternate(c, bestRoute[i - 1]))
                _ = Interlocked.Increment(ref alternateCount);
            if (i > 0 && SmallLeap(c, bestRoute[i - 1]))
                _ = Interlocked.Increment(ref smallLeapCount);
            if (i > 0 && BigLeap(c, bestRoute[i - 1]))
                _ = Interlocked.Increment(ref bigLeapCount);
            if (i > 0 && c == bestRoute[i - 1])
                _ = Interlocked.Increment(ref doubleCount);
            if (i > 1 && c == bestRoute[i - 1] && c == bestRoute[i - 2])
                _ = Interlocked.Increment(ref tripleCount);
            if (i > 2
                && c == bestRoute[i - 1]
                && c == bestRoute[i - 2]
                && c == bestRoute[i - 3])
                _ = Interlocked.Increment(ref quadrupleCount);
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
            $"数排\t{numCount}\t{100.0 * numCount / bestRoute.Length:F3}%",
            $"上排\t{upperCount}\t{100.0 * upperCount / bestRoute.Length:F3}%",
            $"中排\t{midCount}\t{100.0 * midCount / bestRoute.Length:F3}%",
            $"下排\t{lowerCount}\t{100.0 * lowerCount / bestRoute.Length:F3}%",
            $"底排\t{bottomCount}\t{100.0 * bottomCount / bestRoute.Length:F3}%",
            $"总左手\t{leftSum}\t{100.0 * leftSum / bestRoute.Length:F3}%",
            $"总右手\t{rightSum}\t{100.0 * rightSum / bestRoute.Length:F3}%",
            $"偏倚率\t{100.0 * (leftSum - rightSum) / (leftSum + rightSum):F3}%",
            $"左小指\t{left5Count}\t{100.0 * left5Count / bestRoute.Length:F3}%",
            $"左无名\t{left4Count}\t{100.0 * left4Count / bestRoute.Length:F3}%",
            $"左中指\t{left3Count}\t{100.0 * left3Count / bestRoute.Length:F3}%",
            $"左食指\t{left2Count}\t{100.0 * left2Count / bestRoute.Length:F3}%",
            $"右食指\t{right2Count}\t{100.0 * right2Count / bestRoute.Length:F3}%",
            $"右中指\t{right3Count}\t{100.0 * right3Count / bestRoute.Length:F3}%",
            $"右无名\t{right4Count}\t{100.0 * right4Count / bestRoute.Length:F3}%",
            $"右小指\t{right5Count}\t{100.0 * right5Count / bestRoute.Length:F3}%",
            $"拇指键\t{thumbCount}\t{100.0 * thumbCount / bestRoute.Length:F3}%",
            $"两连击\t{doubleCount}\t{100.0 * doubleCount / (bestRoute.Length - 1):F3}%",
            $"三连击\t{tripleCount}\t{100.0 * tripleCount / (bestRoute.Length - 2):F3}%",
            $"四连击\t{quadrupleCount}\t{100.0 * quadrupleCount / (bestRoute.Length - 3):F3}%",
            $"左右互击\t{alternateCount}\t{100.0 * alternateCount / (bestRoute.Length - 1):F3}%",
            $"同指小跨排\t{smallLeapCount}\t{100.0 * smallLeapCount / (bestRoute.Length - 1):F3}%",
            $"同指大跨排\t{bigLeapCount}\t{100.0 * bigLeapCount / (bestRoute.Length - 1):F3}%"
        ];

        static bool DoubleMatch(char c1, string pattern1, char c2, string pattern2)
            => (pattern1.Contains(c1) && pattern2.Contains(c2))
               || (pattern2.Contains(c1) && pattern1.Contains(c2));

        static bool Alternate(char c1, char c2) // 左右互击
            => DoubleMatch(c1, $"{Layout[5]}{Layout[6]}{Layout[7]}{Layout[8]}",
                c2, $"{Layout[9]}{Layout[10]}{Layout[11]}{Layout[12]}");

        static bool SameFinger(char c1, char c2) // 同指
            => DoubleMatch(c1, Layout[5], c2, Layout[5])
                || DoubleMatch(c1, Layout[6], c2, Layout[6])
                || DoubleMatch(c1, Layout[7], c2, Layout[7])
                || DoubleMatch(c1, Layout[8], c2, Layout[8])
                || DoubleMatch(c1, Layout[9], c2, Layout[9])
                || DoubleMatch(c1, Layout[10], c2, Layout[10])
                || DoubleMatch(c1, Layout[11], c2, Layout[11])
                || DoubleMatch(c1, Layout[12], c2, Layout[12]);

        static bool SmallLeap(char c1, char c2) // 同指小跨排
            => SameFinger(c1, c2)
                && (DoubleMatch(c1, Layout[0], c2, Layout[1])
                || DoubleMatch(c1, Layout[1], c2, Layout[2])
                || DoubleMatch(c1, Layout[2], c2, Layout[3]));

        static bool BigLeap(char c1, char c2) // 同指大跨排
            => SameFinger(c1, c2)
                && (DoubleMatch(c1, Layout[0], c2, Layout[2])
                || DoubleMatch(c1, Layout[1], c2, Layout[3])
                || DoubleMatch(c1, Layout[0], c2, Layout[3]));
    }
}
