namespace CodeRace.src;

/// <summary>
/// 编码路径缓冲区
/// </summary>
internal class RouteBuffer
{
    /// <summary>
    /// 编码路径缓冲区：索引为编码终点字符的位置
    /// </summary>
    private readonly HashSet<(string route, double time)>[] _buffer;

    /// <summary>
    /// 编码路径连接器
    /// <summary>
    private readonly RouteConnector _connector;

    /// <summary>
    /// 缓冲区头指针（当前位置）
    /// </summary>
    private int _head;

    /// <summary>
    /// 缓冲区内最远终点位置与当前位置的距离
    /// </summary>
    private int _distance;

    /// <summary>
    /// 暂存的全局最优路径
    /// </summary>
    private (string route, double time) _globalBestRoute;

    /// <summary>
    /// 是否在当前位置连接过编码
    /// </summary>
    internal bool IsConnected { get; private set; }

    /// <summary>
    /// 找不到用时当量的按键组合数量
    /// </summary>
    internal int InvalidKeysCount => _connector.InvalidKeys.Count;

    internal RouteBuffer(int bufferSize, RouteConnector connector)
    {
        if (bufferSize <= 0)
            throw new ArgumentException("编码路径缓冲区大小无效");

        _buffer = new HashSet<(string, double)>[bufferSize];
        _connector = connector;
        _head = 0;
        _distance = 0;
        _globalBestRoute = ("", 0.0);
        IsConnected = false;

        for (int i = 0; i < bufferSize; i++)
            _buffer[i] = [];
    }

    private void Clear()
    {
        foreach (var set in _buffer)
            set.Clear();
        _distance = 0;
        IsConnected = false;
    }

    internal void Next()
    {
        _buffer[_head].Clear();
        _head = (_head + 1) % _buffer.Length;
        _distance--;
        IsConnected = false;
    }

    private (string route, double time) GetLocalBestRoute()
    => _buffer[_head].Count == 0
        ? ("", 0.0)
        : _buffer[_head].OrderBy(static t => t.time)
            .ThenBy(static t => t.route.Length)
            .First();

    internal void Connect(int wordLength, string tailRoute, double tailTime)
    {
        // 取出当前位置的局部最优路径
        var (localBestRoute, localBestTime) = GetLocalBestRoute();

        // 如果局部最优路径太长，且当前集合就是唯一集合（局部最优即全局最优），则缓存
        if (localBestRoute.Length > 500 && _distance == 0)
        {
            _globalBestRoute = _connector.Connect(
                _globalBestRoute.route,
                localBestRoute,
                _globalBestRoute.time,
                localBestTime);
            localBestRoute = "";
            localBestTime = 0.0;
            Clear();
        }

        // 连接编码
        var index = (_head + wordLength) % _buffer.Length;
        _ = _buffer[index].Add(
            _connector.Connect(
                localBestRoute, tailRoute, localBestTime, tailTime));

        // 更新状态
        _distance = Math.Max(_distance, wordLength);
        IsConnected = true;
    }

    internal (string route, double time) GetGlobalBestRoute()
    {
        if (_buffer[_head].Count == 0)
            throw new InvalidOperationException("编码无法到达文本末尾");
        else if (_distance > 0)
            throw new InvalidOperationException("编码超出了文本末尾");

        var (localBestRoute, localBestTime) = GetLocalBestRoute();
        return localBestRoute.Length == 0
            ? _globalBestRoute
            : _connector.Connect(
                _globalBestRoute.route,
                localBestRoute,
                _globalBestRoute.time,
                localBestTime);
    }
}
