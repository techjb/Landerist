using System.Text;

namespace landerist_library.Database;

internal sealed class SqlQueryStack
{
    private readonly StringBuilder _builder = new();
    private readonly object _lock = new();
    private int _count;

    public string? Append(string query, int maximumCount)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumCount);

        lock (_lock)
        {
            _builder.Append(query).Append(' ');
            _count++;
            return _count >= maximumCount ? TakeCore() : null;
        }
    }

    public string? Take()
    {
        lock (_lock)
        {
            return TakeCore();
        }
    }

    private string? TakeCore()
    {
        if (_count == 0)
        {
            return null;
        }

        string query = _builder.ToString();
        _builder.Clear();
        _count = 0;
        return query;
    }
}
