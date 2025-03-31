namespace CodeRace.src;

internal static class FileLoader
{
    internal static bool LoadConfig()
    {
        try
        {
            var configDir = Path.Combine(AppContext.BaseDirectory, "config");
            var layoutPath = Path.Combine(configDir, "layout.txt");
            var timeCostPath = Path.Combine(configDir, "time_cost.txt");
            UsageStatistics.Layout = File.ReadAllLines(layoutPath);
            Encoder.TimeCostMap = new Dictionary<string, double>(4096);
            foreach (var line in File.ReadAllLines(timeCostPath))
            {
                var parts = line.Split('\t');
                if (parts.Length != 2)
                    throw new Exception($"当量文件有格式错误的行：{line}");
                var timeCost = double.TryParse(parts[1], out var result)
                    ? result : throw new Exception($"当量文件无法解析当量：{line}");
                if (Encoder.TimeCostMap.ContainsKey(parts[0]))
                    throw new Exception($"当量文件有重复的键：{parts[0]}");
                Encoder.TimeCostMap.Add(parts[0], timeCost);
            }
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"加载配置出错：{ex.Message}");
            UsageStatistics.Layout = [];
            Encoder.TimeCostMap = [];
            return false;
        }
    }

    internal static bool LoadDict(
        string? path,
        out Dictionary<string, (string, double)>? dict)
    {
        try
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("字典文件不存在！");
            Console.WriteLine("读取字典文件...");
            var items = ReadItems();
            Console.WriteLine($"读取完成，共{items.Length}个条目。去重并生成当量...");
            dict = ParseItems(items);
            Console.WriteLine($"处理完成，共{dict.Count}个最优条目。");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"加载字典出错：{ex.Message}");
            dict = null;
            return false;
        }

        (string word, string code, int priority)[] ReadItems() // 词\t码\t优先级，按优先级降序
        {
            HashSet<(string word, string code, int priority)> items = [];
            foreach (var line in File.ReadAllLines(path))
            {
                var parts = line[..line.IndexOf('#')].Split('\t');
                if (parts.Length == 2)
                    _ = items.Add((parts[0], parts[1], 0));
                else if (parts.Length == 3)
                    _ = items.Add((parts[0], parts[1], int.Parse(parts[2])));
            }
            return [.. items.OrderByDescending(x => x.priority)];
        }

        Dictionary<string, (string, double)> ParseItems((string word, string code, int priority)[] items)
        {
            Dictionary<string, (string, double)> dict = new(items.Length);
            HashSet<string> usedCode = new(items.Length);
            foreach (var (word, code, _) in items)
            {
                var newRealCode = Distinct(code);
                var newTimeCost = Encoder.GetTimeCost(newRealCode);
                if (dict.TryGetValue(word, out (string, double timeCost) old))
                {
                    if (old.timeCost > newTimeCost)
                        dict[word] = (newRealCode, newTimeCost);
                }
                else dict.Add(word, (newRealCode, newTimeCost));
            }
            return dict; // dict中装的每个打法都是该词最优的打法，其余的打法丢弃

            string Distinct(string code)
            {
                var realCode = code;
                var flipCode = ""; // 翻页键
                for (int i = 2; usedCode.Contains(realCode); i++)
                {
                    if (i == 10)
                    {
                        flipCode = $"{flipCode}=";
                        realCode = $"{code}{flipCode}";
                        i = 1;
                    }
                    else realCode = $"{code}{flipCode}{i}";
                }
                _ = usedCode.Add(realCode); // 一定会占掉码位，但不一定会打这个码
                return realCode;
            }
        }
    }
}
