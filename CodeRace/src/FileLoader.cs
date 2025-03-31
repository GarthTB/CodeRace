namespace CodeRace.src;

internal static class FileLoader
{
    internal static bool LoadConfig(
        out string[]? layout,
        out Dictionary<string, float>? timeCostMap)
    {
        try
        {
            var configDir = Path.Combine(AppContext.BaseDirectory, "config");
            var layoutPath = Path.Combine(configDir, "layout.txt");
            var timeCostPath = Path.Combine(configDir, "time_cost.txt");
            layout = File.ReadAllLines(layoutPath);
            timeCostMap = new Dictionary<string, float>(4096);
            foreach (var line in File.ReadAllLines(timeCostPath))
            {
                var parts = line.Split('\t');
                if (parts.Length != 2)
                    throw new Exception($"当量文件有格式错误的行：{line}");
                var timeCost = float.TryParse(parts[1], out var result)
                    ? result : throw new Exception($"当量文件无法解析当量：{line}");
                if (timeCostMap.ContainsKey(parts[0]))
                    throw new Exception($"当量文件有重复的键：{parts[0]}");
                timeCostMap.Add(parts[0], timeCost);
            }
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"加载配置出错：{ex.Message}");
            layout = null;
            timeCostMap = null;
            return false;
        }
    }

    internal static bool LoadDict(
        string? path,
        out Dictionary<string, string>? dict)
    {

    }
}
