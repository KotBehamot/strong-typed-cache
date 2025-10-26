namespace StrongTypedCache.Abstractions;

/// <summary>
/// Represents options for cache configuration.
/// </summary>
public interface ICacheOptions
{
    /// <summary>
    /// Absolute expiration time in seconds.
    /// </summary>
    int AbsoluteExpirationTimeSec { get; }
}