namespace CodeRace.src;

/// <summary>
/// 文件的载入和保存操作
/// </summary>
internal static class FileHelper
{
    #region 加载配置文件

    private static string GetConfigPath(string fileName)
    => Path.Combine(Path.Combine(AppContext.BaseDirectory, "config"), fileName);

    internal static string[] GetLayout()
    {
        Console.WriteLine("加载键盘布局配置...");
        var layout = File.ReadAllLines(GetConfigPath("layout.txt"));
        Console.WriteLine($"加载完成。应为14行，实际共{layout.Length}行。");
        return layout;
    }

    internal static HashSet<(string, string, int)> GetPunctItems()
    {
        Console.WriteLine("加载标点符号配置...");
        var items = GetConfigPath("punct_dict.txt").ReadasHashSet(4096);
        Console.WriteLine($"加载完成。默认为30项，实际共{items.Count}项。");
        return items;
    }

    internal static Dictionary<string, double> GetTimeMap()
    {
        Console.WriteLine("加载击键当量配置...");
        using StreamReader sr = new(GetConfigPath("time_cost.txt"));
        Dictionary<string, double> timeMap = new(4096);
        for (string? line; (line = sr.ReadLine()) != null;)
        {
            var parts = line.Split('\t');
            if (parts.Length != 2)
                continue;

            if (timeMap.TryGetValue(parts[0], out _))
                Console.WriteLine($"当量文件有重复的键：{parts[0]}。只保留首个值。");
            else if (!double.TryParse(parts[1], out var time))
                Console.WriteLine($"无法解析当量文件中此行的当量：{line}。已忽略。");
            else timeMap[parts[0]] = time;
        }
        Console.WriteLine($"加载完成。默认为2116项，实际共{timeMap.Count}项。");
        return timeMap;
    }

    #endregion

    #region 加载词库文件

    /// <summary>
    /// 读取并解析Rime格式的词库文件
    /// </summary>
    private static HashSet<(string word, string code, int priority)> ReadasHashSet(
        this string filePath, int initialCapacity)
    {
        using StreamReader sr = new(filePath);
        HashSet<(string, string, int)> punctItems = new(initialCapacity);
        for (string? line; (line = sr.ReadLine()) != null;)
        {
            // 去掉注释，并按制表符分割
            var parts = line.Split('#')[0].Split('\t');
            if (parts.Length == 2)
                _ = punctItems.Add((parts[0], parts[1], 0));
            else if (parts.Length == 3)
                _ = punctItems.Add((parts[0], parts[1],
                    int.TryParse(parts[2], out int result)
                        ? result
                        : 0));
        }
        return punctItems;
    }

    /// <summary>
    /// 读取词库文件并整合标点符号
    /// </summary>
    /// <returns>
    /// 按首字符分组的总和词库，以及其中词组的最大长度
    /// </returns>
    internal static (Dictionary<char, (string, string, double)[]>, int) GetMasterDict(
        string filePath,
        HashSet<(string word, string code, int priority)> punctItems,
        RouteConnector connector)
    {
        Console.WriteLine("读取词库文件...");
        var dictItems = filePath.ReadasHashSet(65536);
        Console.WriteLine($"读取完成。共{dictItems.Count}项。");
        Console.WriteLine("整合标点符号并生成选重、翻页信息...");
        var sortedDictItems = SortItems(dictItems);
        var sortedPunctItems = SortItems(punctItems);
        var (masterDict, maxWordLength) = ConvertItems(sortedDictItems, sortedPunctItems, connector);
        Console.WriteLine($"整合完成。首字共覆盖{masterDict.Count}个字符。");
        return (masterDict, maxWordLength);
    }

    /// <summary>
    /// 排序条目。顺序：优先级降序、码长升序、词升序、码升序
    /// </summary>
    private static (string word, string code, int priority)[] SortItems(
        HashSet<(string word, string code, int priority)> set)
    => [.. set.OrderByDescending(static x => x.priority)
        .ThenBy(static x => x.code.Length / x.word.Length)
        .ThenBy(static x => x.word)
        .ThenBy(static x => x.code)];

    private static (Dictionary<char, (string, string, double)[]>, int) ConvertItems(
        (string word, string code, int _)[] dictItems, // 优先级已无用，弃掉
        (string word, string code, int _)[] punctItems, // 优先级已无用，弃掉
        RouteConnector connector)
    {
        // 生成并记录唯一编码的方法
        HashSet<string> usedCodes = new(dictItems.Length + punctItems.Length);
        string GetUniqueCode(string code)
        {
            for (int i = 2; usedCodes.Contains(code); i++)
            {
                if (i == 2)
                    code = $"{code}2";
                else if (i < 10)
                    code = $"{code[..^1]}{i}";
                else
                {
                    code = $"{code[..^1]}="; // 等号翻页
                    i = 1;
                }
            }
            _ = usedCodes.Add(code);
            return code;
        }

        // 装填词库并记录最长词组长度的方法
        var maxWordLength = 0;
        Dictionary<string, (string code, double time)> midDict = new(65536);
        void AddItem(string word, string code)
        {
            maxWordLength = Math.Max(maxWordLength, word.Length);
            var uniqueCode = GetUniqueCode(code);
            var newTime = connector.GetTime(uniqueCode);
            if (midDict.TryGetValue(word, out var old))
            {
                if (newTime < old.time)
                    midDict[word] = (uniqueCode, newTime);
            }
            else midDict[word] = (uniqueCode, newTime);
        }

        // 进行装填
        foreach (var (word, code, _) in dictItems)
            AddItem(word, code);
        foreach (var (word, code, _) in punctItems)
            AddItem(word, code);
        if (connector.InvalidKeys.Count == 0)
            Console.WriteLine("编码中没有遇到找不到当量的按键组合。");
        else Console.WriteLine($"编码中遇到{connector.InvalidKeys.Count}个找不到当量的按键组合。");
        Console.WriteLine($"整理后共{midDict.Count}个最优词组。");
        Console.WriteLine($"最大词组长度为{maxWordLength}字符。");

        // 按首字符分组并返回
        var masterDict = midDict.Select(static x => (x.Key, x.Value.code, x.Value.time))
            .GroupBy(static x => x.Key[0])
            .ToDictionary(static x => x.Key, static x => x.ToArray());
        return (masterDict, maxWordLength);
    }

    #endregion

    #region 输出文件

    internal static void Report(string textPath, string name, string[] content)
    {
        try
        {
            Console.WriteLine($"输出{name}...");
            var dir = Path.GetDirectoryName(textPath)
                ?? throw new ArgumentException("无法获取待测文本所在目录！");
            var textFileName = Path.GetFileNameWithoutExtension(textPath);
            var outputPath = Path.Combine(dir, $"{textFileName}_{name}.txt");
            for (int i = 2; File.Exists(outputPath); i++)
                outputPath = Path.Combine(dir, $"{textFileName}_{name}_{i}.txt");
            File.WriteAllLines(outputPath, content);
            Console.WriteLine($"{name}已保存至：{outputPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"无法将{name}保存为文件！错误：{ex.Message}");
            Console.WriteLine("将直接输出到控制台。");
            foreach (var line in content)
                Console.WriteLine(line);
            Console.WriteLine($"{name}输出完毕。");
        }
    }

    #endregion
}
