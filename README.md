# StrongTypedCache

A library for strongly-typed caching with DI support (.NET 8).

## Installation

1. Add a reference to the `StrongTypedCache.Extensions` and `StrongTypedCacheLibrary` projects.
2. Add the `Microsoft.Extensions.DependencyInjection` package if you don't have it already.

## Usage

### Registering in DI

```csharp
using Microsoft.Extensions.DependencyInjection;
using StrongTypedCache.Extensions;

var services = new ServiceCollection();
services.AddStrongTypedInMemoryCache<string, MyType>();
```

You can set the expiration time (in seconds):

```csharp
services.AddStrongTypedInMemoryCache<string, MyType>(absoluteExpirationTimeSec: 600); // 10 minutes
```

### Using the cache

```csharp
using StrongTypedCache.Abstractions;

public class MyService
{
    private readonly ICache<string, MyType> _cache;
    public MyService(ICache<string, MyType> cache)
    {
        _cache = cache;
    }

    public void Example()
    {
        // Add to cache
        _cache.CreateEntry("key1", new MyType());

        // Get from cache
        if (_cache.TryGetValue("key1", out var value))
        {
            // use value
        }

        // Remove from cache
        _cache.Remove("key1");

        // Get all values
        var all = _cache.GetAllValues();
    }
}
```

### Interfaces

- `ICache<TKey, TValue>` – main cache interface.
- `InMemoryCache<TKey, TValue>` – in-memory implementation.

### Projects

- `StrongTypedCache.Abstractions` – interfaces.
- `StrongTypedCacheLibrary` – cache implementation.
- `StrongTypedCache.Extensions` – DI integration.

## Requirements

- .NET 8
- Microsoft.Extensions.DependencyInjection
- Microsoft.Extensions.Caching.Memory

## License

MIT
