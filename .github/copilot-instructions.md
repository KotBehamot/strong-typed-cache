# Copilot Coding Instructions

These instructions describe the coding philosophy, conventions and testing rules used in this repository.
Follow them in every code suggestion, review and generation task.

---

## 1. General Approach

- Always verify the correctness of generated code. Consider multiple solutions before choosing one.
- When given a code example, include a full implementation in the response — not just a description.
- Preserve the notation and style of any example code you receive.
- Apply SOLID principles and Uncle Bob's Clean Code practices by default.

---

## 2. Naming Conventions

| Scope | Convention | Example |
|---|---|---|
| Private fields | `_camelCase` | `_memoryCache`, `_absoluteExpiration`, `_keys` |
| Public properties | `PascalCase` | `AbsoluteExpiration`, `MemoryCache` |
| Public methods | `PascalCase` | `CreateEntry`, `TryGetValue`, `GetAllValues`, `Remove` |
| Local variables | `camelCase` | `items`, `entry`, `options`, `value` |
| Type parameters | `TPascalCase` | `TKey`, `TValue` |
| Interfaces | `IPascalCase` | `ICache<TKey, TValue>`, `ICacheOptions` |
| Discard | `_` only | `TryRemove(key, out _)` |

- **Do not** use snake_case (`my_variable`, `some_method_name`). The only place an underscore appears is
  as the private-field prefix (`_field`) or as the standalone discard symbol (`_`).

### Examples from this codebase

```csharp
// ✅ Correct
private readonly IMemoryCache _memoryCache;
private readonly ConcurrentDictionary<TKey, byte> _keys = new();

public TimeSpan AbsoluteExpiration => _absoluteExpiration;

public List<TValue?> GetAllValues()
{
    var items = new List<TValue?>();
    foreach (var key in _keys.Keys)
    {
        if (_memoryCache.TryGetValue(key, out TValue? value))
            items.Add(value);
        else
            _keys.TryRemove(key, out _);   // discard
    }
    return items;
}

// ❌ Wrong — snake_case
private readonly IMemoryCache memory_cache;
var all_items = new List<TValue?>();
```

---

## 3. SOLID Principles

### Single Responsibility Principle (SRP)
Each class has exactly one reason to change.

```csharp
// ✅ ICache<TKey, TValue> only defines cache operations.
// CacheOptions only carries configuration.
// CacheServiceCollectionExtensions only handles DI registration.

public interface ICache<in TKey, TValue>
{
    bool TryGetValue(TKey key, out TValue value);
    List<TValue?> GetAllValues();
    bool CreateEntry(TKey key, TValue value);
    void Remove(TKey key);
}
```

### Open/Closed Principle (OCP)
Abstractions are open for extension, closed for modification.
New cache strategies implement `ICache<TKey, TValue>` without touching existing code.

### Liskov Substitution Principle (LSP)
Every implementation of `ICache<TKey, TValue>` must behave according to the contract the interface defines.

### Interface Segregation Principle (ISP)
Keep interfaces focused. `ICacheOptions` exposes only what configuration consumers need.

```csharp
public interface ICacheOptions
{
    int AbsoluteExpirationTimeSec { get; }
}
```

### Dependency Inversion Principle (DIP)
Depend on abstractions, not on concrete types.
Inject `ICache<TKey, TValue>`, never `InMemoryCache<TKey, TValue>` directly.

```csharp
// ✅ Correct
public class MyService
{
    private readonly ICache<string, MyType> _cache;

    public MyService(ICache<string, MyType> cache) => _cache = cache;
}

// ❌ Wrong — depends on concrete class
public class MyService
{
    private readonly InMemoryCache<string, MyType> _cache;
}
```

---

## 4. Clean Code (Uncle Bob)

- **Small methods** — a method should do one thing. If it needs a comment to explain *what* it does, extract it.
- **Meaningful names** — `CreateEntry`, `TryGetValue`, `GetAllValues` are self-documenting.
- **No magic numbers** — use named constants or constructor parameters (e.g., `absoluteExpirationTimeSec = 3600`).
- **Guard clauses first** — validate inputs at the top, throw early.

```csharp
public bool CreateEntry(TKey key, TValue value)
{
    if (key is null) throw new ArgumentNullException(nameof(key));

    using var entry = _memoryCache.CreateEntry(key);
    entry.SetValue(value);
    entry.SetAbsoluteExpiration(_absoluteExpiration);
    return true;
}
```

- **Avoid unnecessary comments** — code should read like prose. Add XML docs for public API, not inline comments
  that just repeat the code.
- **Consistent abstraction levels** — don't mix low-level cache manipulation with business logic in the same method.

---

## 5. Testing Rules

### Framework
Use **NUnit** with the `Assert.That` constraint model.

```csharp
// ✅ Correct
Assert.That(cache.CreateEntry(key, value), Is.True);
Assert.That(cache.TryGetValue(key, out var cached), Is.True);
Assert.That(cached, Is.EqualTo(value));
Assert.That(found, Is.False);
Assert.That(values, Has.Count.EqualTo(3));

// ❌ Wrong — MSTest or classic NUnit asserts
Assert.IsTrue(result);
Assert.AreEqual(expected, actual);
```

### Parameterised Tests
Use `[TestCase]` for inline data and `[TestCaseSource]` for external data sets.

```csharp
[Test]
[TestCase(1, "value1")]
[TestCase(2, "value2")]
public void CreateEntry_StoresValue(int key, string data)
{
    var cache = new InMemoryCache<int, DummyValue>();
    var value = new DummyValue { Data = data };
    Assert.That(cache.CreateEntry(key, value), Is.True);
    Assert.That(cache.TryGetValue(key, out var cachedValue), Is.True);
    Assert.That(cachedValue, Is.EqualTo(value));
}

// TestCaseSource example
private static IEnumerable<TestCaseData> ExpirationCases()
{
    yield return new TestCaseData(1, 1200).SetName("Expires_AfterOneSecond");
    yield return new TestCaseData(2, 2200).SetName("Expires_AfterTwoSeconds");
}

[Test]
[TestCaseSource(nameof(ExpirationCases))]
public void Entry_Expires(int expirationSec, int sleepMs)
{
    var cache = new InMemoryCache<int, DummyValue>(expirationSec);
    cache.CreateEntry(1, new DummyValue { Data = "expiring" });
    System.Threading.Thread.Sleep(sleepMs);
    Assert.That(cache.TryGetValue(1, out _), Is.False);
}
```

### Mocking with NSubstitute
Use **NSubstitute** to substitute `ICache<TKey, TValue>` and other interfaces in unit tests.

```csharp
using NSubstitute;

[Test]
public void MyService_ReturnsCachedValue_WhenKeyExists()
{
    var cache = Substitute.For<ICache<string, DummyValue>>();
    var expected = new DummyValue { Data = "cached" };
    cache.TryGetValue("key1", out Arg.Any<DummyValue>())
         .Returns(callInfo =>
         {
             callInfo[1] = expected;
             return true;
         });

    var service = new MyService(cache);
    var result = service.Get("key1");

    Assert.That(result, Is.EqualTo(expected));
}
```

### Test naming
Use the pattern `MethodName_ExpectedBehavior[_WhenCondition]` where `_WhenCondition` is optional and
should only be added when the condition is not already obvious from the method name or the expected behaviour.

```csharp
// Condition is implicit — omit it
CreateEntry_StoresValue
TryGetValue_ReturnsFalseForMissingKey
Entry_Expires_AfterAbsoluteExpiration

// Condition adds meaningful context — include it
CreateEntry_ThrowsArgumentNullException_WhenKeyIsNull
Remove_ThrowsArgumentNullException_WhenKeyIsNull
```

### Test structure
Follow the Arrange–Act–Assert (AAA) layout. Keep each test focused on a single behaviour.

```csharp
[Test]
public void Remove_RemovesEntry()
{
    // Arrange
    var cache = new InMemoryCache<int, DummyValue>();
    var value = new DummyValue { Data = "one" };
    cache.CreateEntry(1, value);

    // Act
    cache.Remove(1);

    // Assert
    Assert.That(cache.TryGetValue(1, out _), Is.False);
}
```

---

## 6. Code Organisation

- Interfaces live in `StrongTypedCache.Abstractions` — keep them thin and stable.
- Implementations live in `StrongTypedCacheLibrary` — reference only the abstractions project.
- DI helpers live in `StrongTypedCache.Extensions` — no business logic, just registration.
- Tests mirror the structure of production code and reside in dedicated test projects.

---

## 7. XML Documentation

All public types and members must have XML documentation comments.

```csharp
/// <summary>
/// Creates or overwrites an entry in the cache.
/// </summary>
/// <param name="key">An object identifying the entry.</param>
/// <param name="value">The value to cache.</param>
/// <returns>True if the entry was created or overwritten.</returns>
/// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is null.</exception>
bool CreateEntry(TKey key, TValue value);
```

---

## 8. Quick Reference Checklist

Before submitting any code, verify:

- [ ] `Assert.That` used in all NUnit tests
- [ ] No snake_case names; `_` only as private-field prefix or standalone discard
- [ ] Every public member has an XML `<summary>` comment
- [ ] Guard clauses (`ArgumentNullException`) at the top of every public method that accepts reference-type parameters
- [ ] Classes depend on interfaces (`ICache<TKey, TValue>`), not on concrete implementations
- [ ] Each class / method has a single, clearly stated responsibility
- [ ] `[TestCase]` or `[TestCaseSource]` used for data-driven scenarios instead of copy-pasted tests
- [ ] NSubstitute used to mock dependencies, not manual fakes
