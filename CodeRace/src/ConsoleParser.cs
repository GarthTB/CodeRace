namespace CodeRace.src;

/// <summary>
/// 控制台输入的解析器
/// </summary>
internal static class ConsoleParser
{
    internal static bool ParseArgs(
        string[] args,
        Dictionary<string, double> timeMap,
        HashSet<(string word, string code, int priority)> punctItems,
        out RouteConnector? connector,
        out Dictionary<char, (string, string, double)[]>? dict,
        out int maxWordLength,
        out FileInfo? textFile)
    {
        try
        {
            var methodIndex = int.Parse(args[0]);
            if (methodIndex is < 0 or > 2)
                throw new ArgumentException("连接方法代号错误");
            connector = new(timeMap, methodIndex);

            (dict, maxWordLength) = FileHelper.GetMasterDict(
                args[1], punctItems, connector);

            textFile = new(args[2]);
            return !textFile.Exists
                ? throw new FileNotFoundException("待测文本文件不存在")
                : textFile.Length == 0
                    ? throw new ArgumentException("待测文本文件为空")
                    : true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"命令行参数解析失败！错误：{ex.Message}");
            Console.WriteLine("请交互式输入参数。");
            connector = null;
            dict = null;
            maxWordLength = 0;
            textFile = null;
            return false;
        }
    }

    internal static RouteConnector GetConnector(
        Dictionary<string, double> timeMap)
    {
        Console.WriteLine("请输入连接方法代号：");
        Console.WriteLine("0: 空格或符号; 1: 无间隔, 2: 键道顶功");
        int methodIndex;
        while (!int.TryParse(Console.ReadLine(), out methodIndex)
            || methodIndex is < 0 or > 2)
            Console.WriteLine("连接方法代号错误。请重新输入：");
        return new(timeMap, methodIndex);
    }

    internal static (Dictionary<char, (string, string, double)[]>, int) GetDict(
        HashSet<(string word, string code, int priority)> punctItems,
        RouteConnector connector)
    {
        Console.WriteLine("请输入词库文件路径：");
        for (; ; )
        {
            try
            {
                return FileHelper.GetMasterDict(
                    Console.ReadLine()
                        ?? throw new FileNotFoundException("文件路径为空"),
                    punctItems,
                    connector);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"无法载入词库！错误：{ex.Message}");
                Console.WriteLine("请重新输入词库文件路径：");
            }
        }
    }

    internal static FileInfo GetTextFile()
    {
        Console.WriteLine("请输入待测文本文件路径：");
        for (; ; )
        {
            try
            {
                FileInfo file = new(Console.ReadLine()
                    ?? throw new FileNotFoundException("文件路径为空"));
                return !file.Exists
                    ? throw new FileNotFoundException("文件不存在")
                    : file.Length == 0
                        ? throw new ArgumentException("文件为空")
                        : file;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"无法打开文件！错误：{ex.Message}");
                Console.WriteLine("请重新输入待测文本文件路径：");
            }
        }
    }

    internal static bool NeedReportInvalidKeys(int count)
    {
        Console.WriteLine($"是否需要输出这{count}个找不到用时当量的按键组合？");
        Console.WriteLine("按Y键确认，其他键取消。");
        return Console.ReadKey().Key == ConsoleKey.Y;
    }
}
