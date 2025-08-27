using NUnit.Framework;
using System;
using System.Collections.Generic;
using Cache;

namespace Cache.Tests
{
    public class DummyValue
    {
        public string Data { get; set; } = string.Empty;
        public override bool Equals(object? obj) => obj is DummyValue dv && dv.Data == Data;
        public override int GetHashCode() => Data.GetHashCode();
    }

    [TestFixture]
    public class InMemoryCacheTests
    {
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

        [Test]
        [TestCase("a", "abc")]
        [TestCase("b", "def")]
        public void TryGetValue_ReturnsCorrectValue(string key, string data)
        {
            var cache = new InMemoryCache<string, DummyValue>();
            var value = new DummyValue { Data = data };
            cache.CreateEntry(key, value);
            var found = cache.TryGetValue(key, out var cachedValue);
            Assert.That(found, Is.True);
            Assert.That(cachedValue, Is.EqualTo(value));
        }

        [Test]
        public void GetAllValues_ReturnsAllCachedValues()
        {
            var cache = new InMemoryCache<int, DummyValue>();
            var v1 = new DummyValue { Data = "one" };
            var v2 = new DummyValue { Data = "two" };
            cache.CreateEntry(1, v1);
            cache.CreateEntry(2, v2);
            Assert.That(cache.GetAllValues(), Is.EquivalentTo(new[] { v1, v2 }));
        }

        [Test]
        public void Remove_RemovesEntry()
        {
            var cache = new InMemoryCache<int, DummyValue>();
            var v = new DummyValue { Data = "one" };
            cache.CreateEntry(1, v);
            cache.Remove(1);
            var found = cache.TryGetValue(1, out var _);
            Assert.That(found, Is.False);
        }

        [Test]
        public void TryGetValue_ReturnsFalseForMissingKey()
        {
            var cache = new InMemoryCache<int, DummyValue>();
            var found = cache.TryGetValue(99, out var _);
            Assert.That(found, Is.False);
        }

        [Test]
        public void CreateEntry_ThrowsArgumentNullException_WhenKeyIsNull()
        {
            var cache = new InMemoryCache<string, DummyValue>();
            var value = new DummyValue { Data = "test" };
            Assert.Throws<ArgumentNullException>(() => cache.CreateEntry(null, value));
        }

        [Test]
        public void CreateEntry_ThrowsArgumentNullException_WhenValueIsNull()
        {
            var cache = new InMemoryCache<int, DummyValue>();
            Assert.Throws<ArgumentNullException>(() => cache.CreateEntry(1, null));
        }

        [Test]
        public void Remove_ThrowsArgumentNullException_WhenKeyIsNull()
        {
            var cache = new InMemoryCache<string, DummyValue>();
            Assert.Throws<ArgumentNullException>(() => cache.Remove(null));
        }

        [Test]
        public void CreateEntry_OverwritesValueForSameKey()
        {
            var cache = new InMemoryCache<int, DummyValue>();
            var v1 = new DummyValue { Data = "first" };
            var v2 = new DummyValue { Data = "second" };
            cache.CreateEntry(1, v1);
            cache.CreateEntry(1, v2);
            Assert.That(cache.TryGetValue(1, out var cached), Is.True);
            Assert.That(cached, Is.EqualTo(v2));
        }

        [Test]
        public void Entry_Expires_AfterAbsoluteExpiration()
        {
            var cache = new InMemoryCache<int, DummyValue>(1); // 1 sekunda
            var v = new DummyValue { Data = "expiring" };
            cache.CreateEntry(1, v);
            System.Threading.Thread.Sleep(1200); // poczekaj aż wygaśnie
            Assert.That(cache.TryGetValue(1, out var _), Is.False);
        }
    }
}