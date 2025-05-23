using CodeRace.src;

namespace CodeRace;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("欢迎使用CodeRace赛码器！");
        Console.WriteLine("版本号：1.2.0 (20250407)");
        Console.WriteLine("作者：GarthTB <g-art-h@outlook.com>");
        Console.WriteLine("源码：https://github.com/GarthTB/CodeRace");

        try
        {
            // 加载配置文件
            var layout = FileHelper.GetLayout();
            var punctItems = FileHelper.GetPunctItems();
            var timeMap = FileHelper.GetTimeMap();

            // 获取其他参数
            if (args.Length != 3
                || !ConsoleParser.ParseArgs(args, timeMap, punctItems,
                    out var connector,
                    out var dict,
                    out var maxWordLength,
                    out var textFile))
            {
                connector = ConsoleParser.GetConnector(timeMap);
                (dict, maxWordLength) = ConsoleParser.GetDict(
                    punctItems, connector);
                textFile = ConsoleParser.GetTextFile();
            }

            // 启动编码器
            var size = Math.Max(16, maxWordLength);
            RouteBuffer buffer = new(size,
                connector ?? throw new ArgumentException("编码连接器未初始化"));
            var (textLength, route, time) = TextEncoder.Encode(
                textFile ?? throw new ArgumentException("文本文件未指定"),
                dict ?? throw new ArgumentException("词库未初始化"),
                buffer);

            // 输出结果
            var report = CodeAnalyzer.Analyze(layout, textLength, route, time);
            FileHelper.Report(textFile.FullName, "最小当量编码报告", report);
            if (connector.InvalidKeys.Count > 0
                && ConsoleParser.NeedReportInvalidKeys(connector.InvalidKeys.Count))
                connector.ReportInvalidKeys(textFile.FullName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"程序异常中止！错误：{ex.Message}");
        }
        finally
        {
            Console.WriteLine("按任意键退出...");
            _ = Console.ReadKey();
        }
    }
}
