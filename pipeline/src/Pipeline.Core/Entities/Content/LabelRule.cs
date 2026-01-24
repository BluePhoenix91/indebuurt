using Pipeline.Core.Enums;

namespace Pipeline.Core.Entities.Content;

/// <summary>
/// Rule for automatically generating labels based on neighborhood data.
/// Evaluated against GIS statistics at build time.
/// </summary>
public class LabelRule
{
    /// <summary>
    /// Primary key. Generated using Guid v7 (time-ordered).
    /// </summary>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// Category for grouping and ordering labels.
    /// </summary>
    public required LabelCategory Category { get; set; }

    /// <summary>
    /// Display text for the label (e.g., "Veel groen").
    /// </summary>
    public required string LabelText { get; set; }

    /// <summary>
    /// Icon CSS class for the label (e.g., "fa-solid fa-leaf").
    /// </summary>
    public required string LabelIcon { get; set; }

    /// <summary>
    /// Field to evaluate from neighborhood statistics.
    /// </summary>
    public required ConditionField ConditionField { get; set; }

    /// <summary>
    /// Comparison operator for the condition.
    /// </summary>
    public required ConditionOperator ConditionOperator { get; set; }

    /// <summary>
    /// Value(s) to compare against.
    /// Single value for most operators (e.g., "10").
    /// Comma-separated for Between operator (e.g., "5,15").
    /// </summary>
    public required string ConditionValue { get; set; }
}
