namespace CodeRace.src;

internal static class FileLoader
{
    internal static bool LoadConfig()
    {
        try
        {
            Console.WriteLine("加载键盘布局和击键当量数据...");
            var configDir = Path.Combine(AppContext.BaseDirectory, "config");
            var layoutPath = Path.Combine(configDir, "layout.txt");
            var timeCostPath = Path.Combine(configDir, "time_cost.txt");
            CodeAnalyzer.Layout = File.ReadAllLines(layoutPath);
            Console.WriteLine($"键盘布局文件加载完成，共{CodeAnalyzer.Layout.Length}行。");
            Encoder.TimeCostMap = new Dictionary<string, double>(4096);
            foreach (var line in File.ReadAllLines(timeCostPath))
            {
                var parts = line.Split('\t');
                if (parts.Length != 2)
                    throw new Exception($"当量文件有格式错误的行：{line}");
                if (Encoder.TimeCostMap.ContainsKey(parts[0]))
                    throw new Exception($"当量文件有重复的键：{parts[0]}");
                if (!double.TryParse(parts[1], out double timeCost))
                    throw new Exception($"无法解析当量文件中此行的当量：{line}");
                Encoder.TimeCostMap.Add(parts[0], timeCost);
            }
            Console.WriteLine($"当量文件加载完成，共{Encoder.TimeCostMap.Count}行。");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"加载配置出错：{ex.Message}");
            CodeAnalyzer.Layout = [];
            Encoder.TimeCostMap = [];
            return false;
        }
    }

    internal static bool LoadDict(
        string? path, out Dictionary<string, (string, double)>? dict)
    {
        try
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("词库文件不存在！");
            Console.WriteLine("读取词库文件...");
            var items = ReadItems();
            Console.WriteLine($"读取完成，共{items.Length}个条目。去重并生成当量...");
            dict = ParseItems(items);
            Console.WriteLine($"处理完成，共{dict.Count}个最优条目。");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"加载词库出错：{ex.Message}");
            dict = null;
            return false;
        }

        (string word, string code, int priority)[] ReadItems()
        {   // 词\t码\t优先级，按优先级降序，再按码长升序，再按词升序
            HashSet<(string word, string code, int priority)> items = [];
            foreach (var line in File.ReadAllLines(path))
            {
                var parts = line.Contains('#')
                    ? line[..line.IndexOf('#')].Split('\t')
                    : line.Split('\t');
                if (parts.Length == 2)
                    _ = items.Add((parts[0], parts[1], 0));
                else if (parts.Length == 3)
                    _ = items.Add((parts[0], parts[1], int.Parse(parts[2])));
            }
            // 加入常见中文标点
            _ = items.Add(("，", ",", 0));
            _ = items.Add(("。", ".", 0));
            _ = items.Add(("、", "/", 0));
            _ = items.Add(("？", "↑/", 0));
            _ = items.Add(("！", "↑1", 0));
            _ = items.Add(("；", ";", 0));
            _ = items.Add(("：", "↑;", 0));
            _ = items.Add(("%", "↑5", 0));
            _ = items.Add(("&", "↑7", 0));
            _ = items.Add(("……", "↑6", 0));
            _ = items.Add(("…", "↑6←", 0)); // 这个要打完省略号再退一格得到
            _ = items.Add(("——", "↑-", 0));
            _ = items.Add(("—", "↑-←", 0)); // 这个要打完破折号再退一格得到
            _ = items.Add(("“", "↑'", 0));
            _ = items.Add(("”", "↑'", 0));
            _ = items.Add(("‘", "'", 0));
            _ = items.Add(("’", "'", 0));
            _ = items.Add(("《", "↑,", 0));
            _ = items.Add(("》", "↑.", 0));
            _ = items.Add(("（", "↑9", 0));
            _ = items.Add(("）", "↑0", 0));
            return [.. items.OrderByDescending(x => x.priority)
                .ThenBy(x => x.code.Length / (double)x.word.Length)
                .ThenBy(x => x.word)];
        }

        Dictionary<string, (string, double)> ParseItems(
            (string word, string code, int priority)[] items)
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
