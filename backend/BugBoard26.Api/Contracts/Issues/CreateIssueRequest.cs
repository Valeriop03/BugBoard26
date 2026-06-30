using BugBoard26.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace BugBoard26.Api.Contracts.Issues;

public class CreateIssueRequest
{
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(4000, MinimumLength = 3)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public IssueType? Type { get; set; }

    public IssuePriority? Priority { get; set; }

    public int? AssignedToId { get; set; }
}
