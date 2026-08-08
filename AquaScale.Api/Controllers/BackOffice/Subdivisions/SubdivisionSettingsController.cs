using System;
using Microsoft.AspNetCore.Mvc;

namespace AquaScale.Api.Controllers.BackOffice.Subdivisions;

[ApiController]
[Route("api/backoffice/subdivisions/{id}")]
public class SubdivisionSettingsController : ControllerBase
{
    public SubdivisionSettingsController()
    {
    }

    // TODO: implement
    [HttpPatch("settings")]
    public IActionResult UpdateSettings(Guid id)
    {
        return StatusCode(501, "Not implemented");
    }

    // TODO: implement
    [HttpPost("billing-cycles")]
    public IActionResult CreateBillingCycle(Guid id)
    {
        return StatusCode(501, "Not implemented");
    }
}
