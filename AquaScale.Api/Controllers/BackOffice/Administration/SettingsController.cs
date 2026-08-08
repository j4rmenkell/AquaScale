using Microsoft.AspNetCore.Mvc;

namespace AquaScale.Api.Controllers.BackOffice.Administration;

[ApiController]
[Route("api/backoffice/settings/global")]
public class SettingsController : ControllerBase
{
    public SettingsController()
    {
    }

    // TODO: implement
    [HttpGet]
    public IActionResult GetSettings()
    {
        return StatusCode(501, "Not implemented");
    }

    // TODO: implement
    [HttpPatch]
    public IActionResult UpdateSettings()
    {
        return StatusCode(501, "Not implemented");
    }
}
