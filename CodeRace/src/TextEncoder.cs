namespace CodeRace.src;

/// <summary>
/// 文本的编码器
/// </summary>
internal static class TextEncoder
{
    /// <summary>
    /// 编码文本
    /// </summary>
    /// <returns>
    /// 文本字数、编码、击键用时当量
    /// </returns>
    internal static (int, string, double) Encode(
        FileInfo textFile,
        Dictionary<char, (string, string, double)[]> dict,
        RouteBuffer buffer)
    {
        Console.WriteLine("计算编码...");
        using StreamReader sr = new(textFile.OpenRead());
        var text = sr.ReadToEnd();
        Console.WriteLine($"共需计算{text.Length}个字。");
        for (int i = 0; i < text.Length; i++)
        {
            if (i % 500 == 0)
                Console.Write($"\r已计算至第{i}字。" +
                    $"遇到{buffer.InvalidKeysCount}个找不到当量的按键组合。");
            if (dict.TryGetValue(text[i], out var subDict))
                foreach (var (word, code, time) in subDict.AsSpan())
                    if (string.CompareOrdinal(text, i, word, 0, word.Length) == 0)
                        buffer.Connect(word.Length, code, time);
            if (!buffer.IsConnected)
                buffer.Connect(1, text[i..(i + 1)], 0.0);
            buffer.Next();
        }

        (string route, double timeSum) = buffer.GetGlobalBestRoute();
        Console.WriteLine($"计算完成。字均码长：{(double)route.Length / text.Length:F8}");
        return (text.Length, route, timeSum);
    }
}
