using Microsoft.VisualStudio.TestTools.UnitTesting;
using StrongTypedCache.Extensions;
using System;
using Microsoft.Extensions.DependencyInjection;
using StrongTypedCache.Abstractions;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading;

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
        public void AddStrongTypedInMemoryCache_WithOptions_RegistersAndHonorsExpiration()
        {
            var services = new ServiceCollection();
            var opts = new CacheOptions { AbsoluteExpirationTimeSec = 1 };
            services.AddStrongTypedInMemoryCache<int, DummyValue>(opts);
            var provider = services.BuildServiceProvider();
            var cache = provider.GetRequiredService<ICache<int, DummyValue>>();

            cache.CreateEntry(10, new DummyValue { Data = "expiring" });
            Thread.Sleep(1200);
            Assert.IsFalse(cache.TryGetValue(10, out var _));
        }

        [TestMethod()]
        public void AddStrongTypedInMemoryCache_WithConfiguration_BindsAndRegisters()
        {
            var data = new Dictionary<string, string?>
            {
                ["StrongTypedCache:AbsoluteExpirationTimeSec"] = "1"
            };
            var config = new ConfigurationBuilder()
                .Add(new TestConfigurationSource(data))
                .Build();

            var services = new ServiceCollection();
            services.AddStrongTypedInMemoryCache<int, DummyValue>(config);
            var provider = services.BuildServiceProvider();
            var cache = provider.GetRequiredService<ICache<int, DummyValue>>();

            cache.CreateEntry(20, new DummyValue { Data = "expiring" });
            Thread.Sleep(1200);
            Assert.IsFalse(cache.TryGetValue(20, out var _));
        }

        [TestMethod()]
        public void AddStrongTypedInMemoryCache_MultipleRegistrations_Coexist()
        {
            var services = new ServiceCollection();
            services.AddStrongTypedInMemoryCache<int, DummyValue>(absoluteExpirationTimeSec: 3600);
            services.AddStrongTypedInMemoryCache<string, DummyValue>(absoluteExpirationTimeSec: 3600);

            var provider = services.BuildServiceProvider();
            var intCache = provider.GetRequiredService<ICache<int, DummyValue>>();
            var stringCache = provider.GetRequiredService<ICache<string, DummyValue>>();

            Assert.IsTrue(intCache.CreateEntry(5, new DummyValue { Data = "five" }));
            Assert.IsTrue(stringCache.CreateEntry("key", new DummyValue { Data = "val" }));

            Assert.IsTrue(intCache.TryGetValue(5, out var iv));
            Assert.AreEqual("five", iv.Data);
            Assert.IsTrue(stringCache.TryGetValue("key", out var sv));
            Assert.AreEqual("val", sv.Data);
        }

        [TestMethod()]
        public void AddStrongTypedInMemoryCache_ThrowsArgumentNullException_WhenServicesIsNull()
        {
            IServiceCollection services = null!;
            Assert.ThrowsException<ArgumentNullException>(() =>
                StrongTypedCache.Extensions.CacheServiceCollectionExtensions.AddStrongTypedInMemoryCache<int, DummyValue>(services));
        }

        [TestMethod()]
        public void AddStrongTypedInMemoryCache_WithOptions_Throws_WhenOptionsIsNull()
        {
            var services = new ServiceCollection();
            CacheOptions options = null!;
            Assert.ThrowsException<ArgumentNullException>(() => services.AddStrongTypedInMemoryCache<int, DummyValue>(options));
        }

        [TestMethod()]
        public void AddStrongTypedInMemoryCache_WithConfiguration_Throws_WhenConfigIsNull()
        {
            var services = new ServiceCollection();
            IConfiguration config = null!;
            Assert.ThrowsException<ArgumentNullException>(() => services.AddStrongTypedInMemoryCache<int, DummyValue>(config));
        }

        // Minimal configuration source/provider to avoid external packages in tests
        private sealed class TestConfigurationSource : IConfigurationSource
        {
            private readonly IDictionary<string, string?> _data;
            public TestConfigurationSource(IDictionary<string, string?> data) => _data = data;
            public IConfigurationProvider Build(IConfigurationBuilder builder) => new TestConfigurationProvider(_data);
        }

        private sealed class TestConfigurationProvider : ConfigurationProvider
        {
            private readonly IDictionary<string, string?> _data;
            public TestConfigurationProvider(IDictionary<string, string?> data) => _data = data;
            public override void Load()
            {
                Data = new Dictionary<string, string?>(_data, StringComparer.OrdinalIgnoreCase);
            }
        }

        public class DummyValue
        {
            public string Data { get; set; } = string.Empty;
            public override bool Equals(object? obj) => obj is DummyValue dv && dv.Data == Data;
            public override int GetHashCode() => Data.GetHashCode();
        }
    }
}