namespace Pipeline.Core.Enums;

/// <summary>
/// Comparison operators for label rule conditions.
/// </summary>
public enum ConditionOperator
{
    GreaterThan,        // >
    LessThan,           // <
    GreaterThanOrEqual, // >=
    LessThanOrEqual,    // <=
    Equal,              // =
    Between             // range (ConditionValue format: "min,max")
}
