using System;
using Microsoft.AspNetCore.Mvc;

namespace AquaScale.Api.Controllers.BackOffice.Subdivisions;

[ApiController]
[Route("api/backoffice/subdivisions/{id}")]
public class SubdivisionsController : ControllerBase
{
    public SubdivisionsController()
    {
    }

    // TODO: implement
    [HttpGet("statistics")]
    public IActionResult GetStatistics(Guid id)
    {
        return StatusCode(501, "Not implemented");
    }

    // TODO: implement
    [HttpGet("map-status")]
    public IActionResult GetMapStatus(Guid id)
    {
        return StatusCode(501, "Not implemented");
    }

    // TODO: implement
    [HttpGet("meter-readings")]
    public IActionResult GetMeterReadings(Guid id)
    {
        return StatusCode(501, "Not implemented");
    }
}
