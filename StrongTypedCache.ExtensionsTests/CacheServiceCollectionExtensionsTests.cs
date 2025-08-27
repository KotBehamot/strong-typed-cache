using Microsoft.VisualStudio.TestTools.UnitTesting;
using StrongTypedCache.Extensions;
using System;
using Microsoft.Extensions.DependencyInjection;
using StrongTypedCache.Abstractions;

namespace StrongTypedCache.Extensions.Tests
{
    [TestClass()]
    public class CacheServiceCollectionExtensionsTests
    {
        [TestMethod()]
        public void AddStrongTypedInMemoryCache_RegistersCacheInDI()
        {
            var services = new ServiceCollection();
            services.AddStrongTypedInMemoryCache<int, DummyValue>();
            var provider = services.BuildServiceProvider();
            var cache = provider.GetService(typeof(ICache<int, DummyValue>)) as ICache<int, DummyValue>;
            Assert.IsNotNull(cache);
            Assert.IsTrue(cache.CreateEntry(1, new DummyValue { Data = "test" }));
            Assert.IsTrue(cache.TryGetValue(1, out var value));
            Assert.AreEqual("test", value.Data);
        }

        [TestMethod()]
        public void AddStrongTypedInMemoryCache_ThrowsArgumentNullException_WhenServicesIsNull()
        {
            IServiceCollection services = null;
            Assert.ThrowsException<ArgumentNullException>(() =>
                StrongTypedCache.Extensions.CacheServiceCollectionExtensions.AddStrongTypedInMemoryCache<int, DummyValue>(services));
        }

        public class DummyValue
        {
            public string Data { get; set; } = string.Empty;
            public override bool Equals(object? obj) => obj is DummyValue dv && dv.Data == Data;
            public override int GetHashCode() => Data.GetHashCode();
        }
    }
}