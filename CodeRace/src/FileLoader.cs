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
        out Dictionary<string, string>? dict)
    {
        try
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("字典文件不存在！");


        }
        catch (Exception ex)
        {
            Console.WriteLine($"加载字典出错：{ex.Message}");
            dict = null;
            return false;
        }
    }
}
