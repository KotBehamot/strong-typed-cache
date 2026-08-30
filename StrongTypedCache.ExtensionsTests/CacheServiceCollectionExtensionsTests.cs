using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StrongTypedCache.Abstractions;
using Cache;
using StrongTypedCache.Extensions;

namespace Cache.Tests
{
    /// <summary>
    /// Tests for the DI registration helpers in <see cref="CacheServiceCollectionExtensions"/>.
    /// </summary>
    [TestClass]
    public class CacheServiceCollectionExtensionsTests
    {
        private sealed class DummyValue
        {
            public string Data { get; set; } = string.Empty;
            public override bool Equals(object? obj) => obj is DummyValue dv && dv.Data == Data;
            public override int GetHashCode() => Data.GetHashCode();
        }

        [TestMethod]
        public void AddStrongTypedInMemoryCache_WithIntExpiration_RegistersCache()
        {
            var services = new ServiceCollection();
            services.AddStrongTypedInMemoryCache<string, DummyValue>(absoluteExpirationTimeSec: 120);

            var provider = services.BuildServiceProvider();
            var cache = provider.GetRequiredService<ICache<string, DummyValue>>();

            Assert.IsNotNull(cache);
            Assert.AreEqual(TimeSpan.FromSeconds(120), ((InMemoryCache<string, DummyValue>)cache).AbsoluteExpiration);
        }

        // Removed the CacheOptions overload test as the CacheOptions class is no longer part of the public API.

        [TestMethod]
        public void AddStrongTypedInMemoryCache_WithConfiguration_RegistersCache()
        {
            var services = new ServiceCollection();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new System.Collections.Generic.Dictionary<string, string>
                {
                    { "StrongTypedCache:AbsoluteExpirationTimeSec", "300" }
                })
                .Build();
            services.AddStrongTypedInMemoryCache<string, DummyValue>(config);

            var provider = services.BuildServiceProvider();
            var cache = provider.GetRequiredService<ICache<string, DummyValue>>();

            Assert.IsNotNull(cache);
            Assert.AreEqual(TimeSpan.FromSeconds(300), ((InMemoryCache<string, DummyValue>)cache).AbsoluteExpiration);
        }
    }
}
