namespace CodeRace;

internal class Program
{
    private static void Main(string[] args)
    {
        try
        {
            Console.WriteLine("欢迎使用CodeRace赛码器！");
            Console.WriteLine("版本号：1.0.0");
            Console.WriteLine("作者：GarthTB <g-art-h@outlook.com>");
            Console.WriteLine("正在加载键盘布局和击键当量数据...");
            if (!src.FileLoader.LoadConfig())
                throw new Exception("未能加载配置文件！");
            Console.WriteLine("加载成功！");
            (var dict, src.Encoder.JoinMethodIndex, var input) = args.Length == 3
                ? ParseArgs(args)
                : GetInput();
            new src.Analyzer(dict, input).Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"程序提前退出，因为发生了错误：{ex.Message}");
        }
        finally
        {
            _ = Console.ReadKey();
        }
    }

    private static (Dictionary<string, string>?, int, string) ParseArgs(string[] args)
    {
        if (!src.FileLoader.LoadDict(args[0], out var dict))
            throw new Exception("未能加载字典文件！");
        if (!int.TryParse(args[1], out var joinMethodCode))
            throw new Exception("未能解析编码连接方法代号！");
        if (!File.Exists(args[2]))
            throw new Exception("待测文本文件不存在！");
        var text = File.ReadAllText(args[2]);
        return (dict, joinMethodCode, text);
    }

    private static (Dictionary<string, string>?, int, string) GetInput()
    {
        Console.WriteLine("请输入字典文件路径：");
        Dictionary<string, string>? dict;
        while (!src.FileLoader.LoadDict(Console.ReadLine(), out dict))
            Console.WriteLine("未能加载字典文件。请重新输入：");

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
        var text = File.ReadAllText(textPath);

        return (dict, joinMethodCode, text);
    }
}
