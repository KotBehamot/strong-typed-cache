using System.Collections;
using System.Reflection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Internal;
using StrongTypedCache.Abstractions;

namespace Cache;

public class InMemoryCache<TKey, TValue> : ICache<TKey, TValue>
    where TValue : new()
{
    private readonly TimeSpan absoluteExpirationTimeSec;
    private readonly IMemoryCache memoryCache;
    private readonly PropertyInfo entriesCollectionProperty;
    private readonly PropertyInfo valueProperty;

    public TimeSpan AbsoluteExpirationTimeSec => absoluteExpirationTimeSec;

    public IMemoryCache MemoryCache => memoryCache;

    public PropertyInfo EntriesCollectionProperty => entriesCollectionProperty;

    public PropertyInfo ValueProperty => valueProperty;

    public InMemoryCache(int absoluteExpirationTimeSec = 3600)
    {
        var options = new MemoryCacheOptions();
        options.Clock = new SystemClock();
        memoryCache = new MemoryCache(options);
        this.absoluteExpirationTimeSec = new TimeSpan(absoluteExpirationTimeSec);
        
        entriesCollectionProperty = typeof(MemoryCache).GetProperty("EntriesCollection",
            BindingFlags.NonPublic | BindingFlags.Instance);
         valueProperty = typeof(TValue).GetProperty("value");

    }

    public List<TValue> GetAllValues()
    {
        var collection = EntriesCollectionProperty.GetValue(MemoryCache) as ICollection;
        var items = new List<TValue>();
        if (collection != null)
            foreach (var item in collection)
            {
                var val = ValueProperty.GetValue(item);
                items.Add((TValue)val);
            }

        return items;
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        return MemoryCache.TryGetValue(key, out value);
    }

    public bool CreateEntry(TKey key, TValue value)
    {
        if (key is null) throw new ArgumentNullException(nameof(key));
        if (value is null) throw new ArgumentNullException(nameof(value));
        var entry = MemoryCache.CreateEntry(key);
        entry.SetValue(value);
        entry.SetAbsoluteExpiration(AbsoluteExpirationTimeSec);
        return true;
    }

    public void Remove(TKey key)
    {
        if (key is null) throw new ArgumentNullException(nameof(key)); 
        MemoryCache.Remove(key);
    }
}