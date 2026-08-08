using Microsoft.AspNetCore.Mvc;

namespace AquaScale.Api.Controllers.BackOffice.Management;

[ApiController]
[Route("api/backoffice/dashboard/overview")]
public class DashboardController : ControllerBase
{
    public DashboardController()
    {
    }

    // TODO: implement
    [HttpGet]
    public IActionResult GetOverview()
    {
        return StatusCode(501, "Not implemented");
    }
}
