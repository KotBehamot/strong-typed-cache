using Microsoft.Extensions.DependencyInjection;
using StrongTypedCache.Abstractions;
using Cache;

namespace StrongTypedCache.Extensions;

/// <summary>
/// Extension methods for registering strong-typed in-memory cache in the DI container.
/// </summary>
public static class CacheServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="InMemoryCache{TKey, TValue}"/> as the implementation of <see cref="ICache{TKey, TValue}"/> in the DI container.
    /// </summary>
    /// <typeparam name="TKey">The type of the cache key.</typeparam>
    /// <typeparam name="TValue">The type of the cache value.</typeparam>
    /// <param name="services">The service collection to add the cache to.</param>
    /// <param name="absoluteExpirationTimeSec">Absolute expiration time in seconds (default: 3600).</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddStrongTypedInMemoryCache<TKey, TValue>(
        this IServiceCollection services,
        int absoluteExpirationTimeSec = 3600)
        where TValue : new()
    {
        if (services == null) throw new ArgumentNullException(nameof(services));
        services.AddSingleton<ICache<TKey, TValue>>(_ =>
            new InMemoryCache<TKey, TValue>(absoluteExpirationTimeSec));
        return services;
    }
}
