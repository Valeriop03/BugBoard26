using BugBoard26.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace BugBoard26.Api.Contracts.Issues;

public class UpdateIssueStatusRequest
{
    [Required]
    [EnumDataType(typeof(IssueStatus))]
    public IssueStatus? Status { get; set; }
}
