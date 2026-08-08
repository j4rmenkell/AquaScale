using System;
using Microsoft.AspNetCore.Mvc;

namespace AquaScale.Api.Controllers.FieldOperations;

[ApiController]
[Route("api/field/meters/{id}")]
public class MetersController : ControllerBase
{
    public MetersController()
    {
    }

    // TODO: implement
    [HttpGet("latest")]
    public IActionResult GetLatest(Guid id)
    {
        return StatusCode(501, "Not implemented");
    }

    // TODO: implement
    [HttpPost("report")]
    public IActionResult Report(Guid id)
    {
        return StatusCode(501, "Not implemented");
    }
}
