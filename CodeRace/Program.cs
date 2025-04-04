namespace CodeRace;

internal class Program
{
    private static void Main(string[] args)
    {
        try
        {
            Console.WriteLine("欢迎使用CodeRace赛码器！");
            Console.WriteLine("版本号：1.1.0 (20250403)");
            Console.WriteLine("作者：GarthTB <g-art-h@outlook.com>");
            if (!src.FileLoader.LoadConfig())
                throw new Exception("未能加载配置文件！");
            (var dict, src.Encoder.JoinMethodIndex, var textPath) = args.Length == 3
                ? ParseArgs(args)
                : GetInput();
            new src.Core(dict, textPath).Run();
            Console.WriteLine("赛码器运行完毕！");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"程序提前中止，因为发生了错误：{ex.Message}");
        }
        finally
        {
            Console.WriteLine("按任意键退出...");
            _ = Console.ReadKey();
        }
    }

    private static (Dictionary<string, (string, double)>?, int, string) ParseArgs(string[] args)
        => !src.FileLoader.LoadDict(args[0], out var dict)
            ? throw new Exception("未能加载词库文件！")
            : !int.TryParse(args[1], out var joinMethodCode)
                ? throw new Exception("未能解析编码连接方法代号！")
                : !File.Exists(args[2])
                    ? throw new Exception("待测文本文件不存在！")
                    : (dict, joinMethodCode, args[2]);

    private static (Dictionary<string, (string, double)>?, int, string) GetInput()
    {
        Console.WriteLine("请输入词库文件路径：");
        Dictionary<string, (string, double)>? dict;
        while (!src.FileLoader.LoadDict(Console.ReadLine(), out dict))
            Console.WriteLine("未能加载词库文件。请重新输入：");

        Console.WriteLine("请输入编码连接方法代号：");
        Console.WriteLine("0: 空格或标点; 1: 无间隔; 2: 键道顶功");
        int joinMethodCode;
        while (!int.TryParse(Console.ReadLine(), out joinMethodCode)
            || joinMethodCode is < 0 or > 2)
            Console.WriteLine("连接方法代号错误。请重新输入：");

        Console.WriteLine("请输入待测文本路径：");
        string? textPath;
        while (!File.Exists(textPath = Console.ReadLine()))
            Console.WriteLine("待测文本文件不存在。请重新输入：");

        return (dict, joinMethodCode, textPath);
    }
}
