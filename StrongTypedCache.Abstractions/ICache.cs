namespace StrongTypedCache.Abstractions;

/// <summary>
/// Defines a generic cache interface.
/// </summary>
/// <typeparam name="TKey">Type of the cache key.</typeparam>
/// <typeparam name="TValue">Type of the cache value.</typeparam>
public interface ICache<in TKey, TValue>
{
    /// <summary>
    /// Gets the item associated with this key if present.
    /// </summary>
    /// <param name="key">An object identifying the requested entry.</param>
    /// <param name="value">The located value or null.</param>
    /// <returns>True if the key was found.</returns>
    bool TryGetValue(TKey key, out TValue value);

    /// <summary>
    /// Gets all values from the cache.
    /// </summary>
    /// <returns>List of all values in the cache.</returns>
    List<TValue?> GetAllValues();

    /// <summary>
    /// Creates or overwrites an entry in the cache.
    /// </summary>
    /// <param name="key">An object identifying the entry.</param>
    /// <param name="value">The value to cache.</param>
    /// <returns>True if the entry was created or overwritten.</returns>
    bool CreateEntry(TKey key, TValue value);

    /// <summary>
    /// Removes the object associated with the given key.
    /// </summary>
    /// <param name="key">An object identifying the entry.</param>
    void Remove(TKey key);
}