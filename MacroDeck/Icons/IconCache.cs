using System.Collections.Concurrent;
using System.Drawing;

namespace SuchByte.MacroDeck.Icons;

public static class IconCache
{
    private const int DefaultMaxEntries = 200;
    private static readonly ConcurrentDictionary<string, CacheEntry> _cache = new();
    private static readonly object _lock = new();

    public static int MaxEntries { get; set; } = DefaultMaxEntries;

    public static bool TryGet(string key, out Image? image)
    {
        if (_cache.TryGetValue(key, out var entry))
        {
            entry.LastAccess = DateTime.UtcNow;
            image = entry.Image;
            return true;
        }

        image = null;
        return false;
    }

    public static void Set(string key, Image image)
    {
        var entry = new CacheEntry
        {
            Image = (Image)image.Clone(),
            LastAccess = DateTime.UtcNow
        };

        _cache[key] = entry;

        if (_cache.Count > MaxEntries)
        {
            EvictOldest();
        }
    }

    public static int Count => _cache.Count;

    private static void EvictOldest()
    {
        lock (_lock)
        {
            if (_cache.Count <= MaxEntries) return;

            var oldest = _cache
                .OrderBy(kvp => kvp.Value.LastAccess)
                .Take(_cache.Count - MaxEntries)
                .ToList();

            foreach (var kvp in oldest)
            {
                if (_cache.TryRemove(kvp.Key, out var entry))
                {
                    entry.Image?.Dispose();
                }
            }
        }
    }

    public static void Clear()
    {
        lock (_lock)
        {
            foreach (var kvp in _cache)
            {
                kvp.Value.Image?.Dispose();
            }
            _cache.Clear();
        }
    }

    private class CacheEntry
    {
        public Image Image { get; set; } = null!;
        public DateTime LastAccess { get; set; }
    }
}
