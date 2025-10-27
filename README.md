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

### Nullable values support

Cache now supports nullable value types:

```csharp
// String cache with nullable values
services.AddStrongTypedInMemoryCache<int, string?>();

// Later in code:
cache.CreateEntry(1, null); // ? Allowed!
if (cache.TryGetValue(1, out var value))
{
    // value can be null
    Console.WriteLine(value?.Length ?? 0);
}
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

### Key constraints

?? **Important**: Keys cannot be null (enforced by `notnull` constraint):

```csharp
// ? This will cause compilation error
services.AddStrongTypedInMemoryCache<string?, MyType>();

// ? Use non-nullable key types
services.AddStrongTypedInMemoryCache<string, MyType>();
services.AddStrongTypedInMemoryCache<int, MyType>();
services.AddStrongTypedInMemoryCache<Guid, MyType>();
```

### Interfaces

- `ICache<TKey, TValue>` – main cache interface.
- `InMemoryCache<TKey, TValue>` – in-memory implementation.

### Projects

- `StrongTypedCache.Abstractions` – interfaces.
- `StrongTypedCacheLibrary` – cache implementation.
- `StrongTypedCache.Extensions` – DI integration.
- `StrongTypedCache.Benchmarks` – performance benchmarks using BenchmarkDotNet.

## Requirements

- .NET 8
- Microsoft.Extensions.DependencyInjection
- Microsoft.Extensions.Caching.Memory

## License

MIT
