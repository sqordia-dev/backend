using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.Admin;

public class CreateQuestionTemplateRequest
{
    [Required]
    [StringLength(1000, MinimumLength = 5)]
    public required string QuestionText { get; set; }

    [StringLength(1000)]
    public string? QuestionTextEN { get; set; }

    [StringLength(2000)]
    public string? HelpText { get; set; }

    [StringLength(2000)]
    public string? HelpTextEN { get; set; }

    [Required]
    public required string QuestionType { get; set; }

    [Required]
    [Range(1, 5)]
    public required int StepNumber { get; set; }

    public string? PersonaType { get; set; }

    [Required]
    [Range(0, 100)]
    public required int Order { get; set; }

    public bool IsRequired { get; set; } = true;

    [StringLength(100)]
    public string? Section { get; set; }

    [StringLength(50)]
    public string? Icon { get; set; }

    public string? Options { get; set; }
    public string? OptionsEN { get; set; }
    public string? ValidationRules { get; set; }
    public string? ConditionalLogic { get; set; }
}
