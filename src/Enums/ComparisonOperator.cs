using System.Runtime.Serialization;

namespace berrybeat.Neo4j.OGM.Enums;

public enum ComparisonOperator
{
    /// <summary>
    /// Translates to =
    /// </summary>
    Equals,
    /// <summary>
    /// Translates to <>
    /// </summary>
    NotEquals,
    /// <summary>
    /// Translates to <c> a > b </c>
    /// </summary>
    GreaterThan,
    /// <summary>
    /// Translates to <c> a < b </c>
    /// </summary>
    LessThan,
    /// <summary>
    /// Translates to <c> a >= b </c>
    /// </summary>
    GreaterThanOrEquals,
    /// <summary>
    /// Translates to <c> a <= b </c>
    /// </summary>
    LessThanOrEquals,
    //IS NULL and IS NOT NULL will be passed with different methods
}