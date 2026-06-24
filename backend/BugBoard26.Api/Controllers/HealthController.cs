using Microsoft.AspNetCore.Mvc;

namespace BugBoard26.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "ok",
            service = "BugBoard26.Api"
        });
    }
}
