namespace StrongTypedCache.Abstractions;

/// <summary>
/// Default implementation of ICacheOptions for configuration binding.
/// </summary>
public class CacheOptions : ICacheOptions
{
    /// <summary>
    /// Absolute expiration time in seconds (default 3600).
    /// </summary>
    public int AbsoluteExpirationTimeSec { get; set; } = 3600;
}
