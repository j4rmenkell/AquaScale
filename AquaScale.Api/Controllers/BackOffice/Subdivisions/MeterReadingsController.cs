using System;
using Microsoft.AspNetCore.Mvc;

namespace AquaScale.Api.Controllers.BackOffice.Subdivisions;

[ApiController]
[Route("api/backoffice/meter-readings")]
public class MeterReadingsController : ControllerBase
{
    public MeterReadingsController()
    {
    }

    // TODO: implement
    [HttpGet("{id}")]
    public IActionResult GetById(Guid id)
    {
        return StatusCode(501, "Not implemented");
    }

    // TODO: implement
    [HttpPatch("{id}/status")]
    public IActionResult UpdateStatus(Guid id)
    {
        return StatusCode(501, "Not implemented");
    }
}
