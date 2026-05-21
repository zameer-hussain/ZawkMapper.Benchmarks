namespace ZawkMapper.Benchmarks.Mapping;

/// <summary>
/// Describes which ZawkMapper member mapping API is used for the runtime benchmark registration.
/// </summary>
public enum ZawkRuntimeMapStyle
{
    /// <summary>
    /// Uses MapField for every mapped member. This is the most flexible style and may pay conversion checks.
    /// </summary>
    FlexibleMapField,

    /// <summary>
    /// Uses MapFieldDirect where direct assignment is valid.
    /// The nested collection bridge still uses MapField because List&lt;OrderItem&gt; is not directly assignable to List&lt;OrderLineDto&gt;.
    /// </summary>
    DirectMapFieldDirect,

    /// <summary>
    /// Uses MapFieldStrict where source expression and destination member have the same compile-time type.
    /// The nested collection bridge still uses MapField because List&lt;OrderItem&gt; is not the same type as List&lt;OrderLineDto&gt;.
    /// </summary>
    StrictMapFieldStrict,

    /// <summary>
    /// Uses the current realistic public style: direct mapping for simple same-type assignments and flexible mapping for computed, converted, or nested members.
    /// </summary>
    MixedCurrentStyle
}
