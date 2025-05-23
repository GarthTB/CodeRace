namespace CodeRace.src;

/// <summary>
/// 最终结果编码的分析器
/// </summary>
internal static class CodeAnalyzer
{
    internal static string[] Analyze(
        string[] layout, int textLength, string route, double time)
    {
        // 简单分析
        var codeLength = (double)route.Length / textLength;
        var timePerChar = time / textLength;
        var timePerKey = time / route.Length;

        // 简单返回
        if (layout.Length != 14)
        {
            Console.WriteLine("键盘布局配置错误，将只进行简单分析。");
            return [
                route,
                "---以上为最优编码路径，以下为简单分析结果---",
                $"字数\t{textLength}",
                $"码数\t{route.Length}",
                $"当量\t{time:F1}",
                $"字均码长\t{codeLength:F8}",
                $"字均当量\t{timePerChar:F4}",
                $"码均当量\t{timePerKey:F4}",
            ];
        }

        // 完整分析的变量
        var partsCount = new int[14]; // 每组码的计数
        var smallLeapCount = 0; // 同指跨1排
        var middleLeapCount = 0; // 同指跨2排
        var largeLeapCount = 0; // 同指跨3排
        var doubleCount = 0; // 同键按2次
        var tripleCount = 0; // 同键按3次
        var quadrupleCount = 0; // 同键按4次
        var quintupleCount = 0; // 同键按5次
        var turnsCount = 0; // 左右左和右左右的次数之和

        #region 完整分析的方法

        bool DoubleContains(string s1, string s2, char c1, char c2)
            => (s1.Contains(c1) && s2.Contains(c2))
            || (s1.Contains(c2) && s2.Contains(c1));

        bool SameFinger(char c1, char c2)
            => DoubleContains(layout[5], layout[5], c1, c2)
            || DoubleContains(layout[6], layout[6], c1, c2)
            || DoubleContains(layout[7], layout[7], c1, c2)
            || DoubleContains(layout[8], layout[8], c1, c2)
            || DoubleContains(layout[9], layout[9], c1, c2)
            || DoubleContains(layout[10], layout[10], c1, c2)
            || DoubleContains(layout[11], layout[11], c1, c2)
            || DoubleContains(layout[12], layout[12], c1, c2);

        bool SmallLeap(char c1, char c2)
            => DoubleContains(layout[0], layout[1], c1, c2)
            || DoubleContains(layout[1], layout[2], c1, c2)
            || DoubleContains(layout[2], layout[3], c1, c2);

        bool MiddleLeap(char c1, char c2)
            => DoubleContains(layout[0], layout[2], c1, c2)
            || DoubleContains(layout[1], layout[3], c1, c2);

        bool LargeLeap(char c1, char c2)
            => DoubleContains(layout[0], layout[3], c1, c2);

        var leftKeys = $"{layout[5]}{layout[6]}{layout[7]}{layout[8]}";
        var rightKeys = $"{layout[9]}{layout[10]}{layout[11]}{layout[12]}";

        bool Turns(char c1, char c2, char c3)
            => (leftKeys.Contains(c1) && rightKeys.Contains(c2) && leftKeys.Contains(c3))
            || (rightKeys.Contains(c1) && leftKeys.Contains(c2) && rightKeys.Contains(c3));

        void Count1Char(char c)
        {
            for (int i = 0; i < 14; i++)
                if (layout[i].Contains(c))
                    _ = Interlocked.Increment(ref partsCount[i]);
        }

        void Count2Chars(char c1, char c2)
        {
            if (c1 == c2)
                _ = Interlocked.Increment(ref doubleCount);
            else if (SameFinger(c1, c2))
            {
                if (SmallLeap(c1, c2))
                    _ = Interlocked.Increment(ref smallLeapCount);
                else if (MiddleLeap(c1, c2))
                    _ = Interlocked.Increment(ref middleLeapCount);
                else if (LargeLeap(c1, c2))
                    _ = Interlocked.Increment(ref largeLeapCount);
            }
        }

        void Count3Chars(char c1, char c2, char c3)
        {
            if (c1 == c2 && c2 == c3)
                _ = Interlocked.Increment(ref tripleCount);
            else if (Turns(c1, c2, c3))
                _ = Interlocked.Increment(ref turnsCount);
        }

        #endregion

        // 进行完整分析
        _ = Parallel.For(0, route.Length, i =>
        {
            Count1Char(route[i]);
            if (i > 0)
                Count2Chars(route[i], route[i - 1]);
            if (i > 1)
                Count3Chars(route[i], route[i - 1], route[i - 2]);
            if (i > 2
                && route[i] == route[i - 1]
                && route[i] == route[i - 2]
                && route[i] == route[i - 3])
                _ = Interlocked.Increment(ref quadrupleCount);
            if (i > 3
                && route[i] == route[i - 1]
                && route[i] == route[i - 2]
                && route[i] == route[i - 3]
                && route[i] == route[i - 4])
                _ = Interlocked.Increment(ref quintupleCount);
        });

        // 获取并返回结果
        var leftCount = partsCount[5] + partsCount[6] + partsCount[7] + partsCount[8];
        var rightCount = partsCount[9] + partsCount[10] + partsCount[11] + partsCount[12];

        doubleCount -= tripleCount;
        tripleCount -= quadrupleCount;
        quadrupleCount -= quintupleCount;

        var deviationReport = leftCount + rightCount == 0
            ? "双手键数之和为0，无法计算偏倚率。"
            : $"偏倚率\t{100.0 * (leftCount - rightCount) / (leftCount + rightCount):F3}%";

        string GenReport(string name, int count, int involvedKeyCount)
            => $"{name}\t{count}\t{100.0 * count / (route.Length - involvedKeyCount + 1):F3}%";

        return
        [
            route,
            "---以上为最优编码路径，以下为完整分析结果---",
            $"字数\t{textLength}",
            $"码数\t{route.Length}",
            $"当量\t{time:F1}",
            $"字均码长\t{codeLength:F8}",
            $"字均当量\t{timePerChar:F4}",
            $"码均当量\t{timePerKey:F4}",
            GenReport("总左手", leftCount, 1),
            GenReport("总右手", rightCount, 1),
            deviationReport,
            GenReport("数排", partsCount[0], 1),
            GenReport("上排", partsCount[1], 1),
            GenReport("中排", partsCount[2], 1),
            GenReport("下排", partsCount[3], 1),
            GenReport("底排", partsCount[4], 1),
            GenReport("左小指", partsCount[5], 1),
            GenReport("左无名", partsCount[6], 1),
            GenReport("左中指", partsCount[7], 1),
            GenReport("左食指", partsCount[8], 1),
            GenReport("右食指", partsCount[9], 1),
            GenReport("右中指", partsCount[10], 1),
            GenReport("右无名", partsCount[11], 1),
            GenReport("右小指", partsCount[12], 1),
            GenReport("拇指键", partsCount[13], 1),
            GenReport("同指跨1排", smallLeapCount, 2),
            GenReport("同指跨2排", middleLeapCount, 2),
            GenReport("同指跨3排", largeLeapCount, 2),
            GenReport("两连击", doubleCount, 2),
            GenReport("三连击", tripleCount, 3),
            GenReport("四连击", quadrupleCount, 4),
            $"更多连击\t{quintupleCount}",
            GenReport("左右互击", turnsCount, 3),
        ];
    }
}
