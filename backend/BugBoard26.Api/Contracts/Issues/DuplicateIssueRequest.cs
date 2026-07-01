using System.ComponentModel.DataAnnotations;

namespace BugBoard26.Api.Contracts.Issues;

public class DuplicateIssueRequest
{
    [Required]
    public int OriginalIssueId { get; set; }
}
