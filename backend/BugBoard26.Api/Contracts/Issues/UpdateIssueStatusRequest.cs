using BugBoard26.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace BugBoard26.Api.Contracts.Issues;

public class UpdateIssueStatusRequest
{
    [Required]
    public IssueStatus? Status { get; set; }
}
