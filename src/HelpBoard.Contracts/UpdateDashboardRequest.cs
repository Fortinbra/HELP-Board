using System.ComponentModel.DataAnnotations;

namespace HelpBoard.Contracts;

/// <summary>Replacement dashboard content and the revision on which the edit was based.</summary>
public sealed record UpdateDashboardRequest : IValidatableObject
{
    [Required, StringLength(100)]
    public required string Name { get; init; }

    public Guid Revision { get; init; }

    [Required]
    public required IReadOnlyList<DashboardWidget> Widgets { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Revision == Guid.Empty)
        {
            yield return new ValidationResult("A dashboard revision is required.", [nameof(Revision)]);
        }

        if (!DashboardLayout.IsValid(Widgets))
        {
            yield return new ValidationResult("The widget layout or content is invalid.", [nameof(Widgets)]);
        }
    }
}
