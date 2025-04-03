# [CodeRace 🐎 赛码器](https://github.com/GarthTB/CodeRace)

[![Framework](https://img.shields.io/badge/.NET-9.0-blue)](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
[![Version](https://img.shields.io/badge/release-1.0.0-brightgreen)](https://github.com/GarthTB/CodeRace/releases)
[![License](https://img.shields.io/badge/License-Apache%202.0-blue)](https://www.apache.org/licenses/LICENSE-2.0)

轻松计算字数上十万的文章在自定义的键盘布局下、用特定输入法，击键时间当量[1]最小的打法，并简单分析这个最优的编码。

[1]陈一凡,张鹿,周志农.键位相关速度当量的研究[J].中文信息学报,1990,(04):12-18+11.

## 控制台参数

```
[词库文件路径] [编码连接方法代号] [待测文本文件路径]
```

- 词库按照RIME格式，每行格式为`词\t码[\t优先级]`，用`#`号注释。
- 编码连接方法代号为`0: 空格或标点（最常见）; 1: 无间隔; 2: 键道顶功`

*也可以交互式输入参数。*

## 配置文件

配置文件存放在程序同目录的config文件夹中，每次启动时自动加载。包含一个定义键盘布局的`layout.txt`文件和一个定义当量的`time_cost.txt`文件。

`layout.txt`共有14行。每一行分别为：

```
[数字排的码元]
[上排的码元]
[中排的码元]
[下排的码元]
[底排的码元]
[左手小指的码元]
[左手无名指的码元]
[左手中指的码元]
[左手食指的码元]
[右手食指的码元]
[右手中指的码元]
[右手无名指的码元]
[右手小指的码元]
[拇指的码元]
```

## 注意

- 载入词库时会自动将常见的中文标点符号加入词库。因为默认的当量文件中没有shift键（编码为↑）和退格键（编码为←），所以控制台会出现17行找不到当量的报告。最终路径不会受此影响，可以忽略。
- 词库在载入过程中会自动计算选重和翻页键。词库中的条目会依次按`优先级降序、码长升序、词升序`来争夺码位。词库文件本身的条目顺序无效。
- 码位被占用不代表这个打法会被使用。有多个编码的词，永远只会使用当量最小的编码。
- 分析报告中，`偏倚率 = 100% * (左右手键数的差/左右手键数的和)`

## 发布日志

### v1.0.0 - 20250402

- 首个发布！