using System.ComponentModel.DataAnnotations;

namespace BugBoard26.Api.Contracts.Issues;

public class DuplicateIssueRequest
{
    [Required]
    [Range(1, int.MaxValue)]
    public int? OriginalIssueId { get; set; }
}
