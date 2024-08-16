
namespace StrongTypedCache.Abstractions;

public interface ICache<in TKey, TValue>
{
    /// <summary>
    ///     Gets the item associated with this key if present.
    /// </summary>
    /// <param name="key">An object identifying the requested entry.</param>
    /// <param name="value">The located value or null.</param>
    /// <returns>True if the key was found.</returns>
    bool TryGetValue(TKey key, out TValue value);

    List<TValue?> GetAllValues();

    /// <summary>
    ///     Create or overwrite an entry in the cache.
    /// </summary>
    /// <param name="key">An object identifying the entry.</param>
    /// <returns>The newly created <see cref="ICacheEntry" /> instance.</returns>
    bool CreateEntry(TKey key, TValue value);

    /// <summary>
    ///     Removes the object associated with the given key.
    /// </summary>
    /// <param name="key">An object identifying the entry.</param>
    void Remove(TKey key);
}