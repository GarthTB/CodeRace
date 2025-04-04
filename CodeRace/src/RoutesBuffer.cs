namespace CodeRace.src;

internal class RoutesBuffer
{
    private readonly HashSet<(string code, double timeCost)>[] _routes;
    private int _head;

    internal RoutesBuffer(int bufferSize)
    {
        _routes = new HashSet<(string code, double timeCost)>[bufferSize];
        _routes[0] = [("", 0)];
        for (int i = 1; i < bufferSize; i++)
            _routes[i] = [];
    }

    internal bool GetBestRoute(out string bestRoute, out double timeCost)
    {
        (bestRoute, timeCost) = _routes[_head].Count > 0
            ? _routes[_head].MinBy(x => x.timeCost)
            : ("", -1);
        return _routes[_head].Count > 0;
    }

    internal void Add(int relativeIndex, (string code, double timeCost) tuple)
    {
        var index = (_head + relativeIndex) % _routes.Length;
        _ = _routes[index].Add(tuple);
    }

    internal void Next()
    {
        _routes[_head].Clear();
        _head = (_head + 1) % _routes.Length;
    }

    internal void Reset()
    {
        foreach (var set in _routes)
            set.Clear();
        _ = _routes[_head].Add(("", 0));
    }
}
