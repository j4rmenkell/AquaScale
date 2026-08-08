using System;
using Microsoft.AspNetCore.Mvc;

namespace AquaScale.Api.Controllers.BackOffice.Subdivisions;

[ApiController]
[Route("api/backoffice/subdivisions/{id}/notices")]
public class SubdivisionNoticesController : ControllerBase
{
    public SubdivisionNoticesController()
    {
    }

    // TODO: implement
    [HttpGet]
    public IActionResult GetNotices(Guid id)
    {
        return StatusCode(501, "Not implemented");
    }

    // TODO: implement
    [HttpPost]
    public IActionResult CreateNotice(Guid id)
    {
        return StatusCode(501, "Not implemented");
    }
}
