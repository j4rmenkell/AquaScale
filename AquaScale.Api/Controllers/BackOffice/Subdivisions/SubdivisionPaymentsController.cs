using System;
using Microsoft.AspNetCore.Mvc;

namespace AquaScale.Api.Controllers.BackOffice.Subdivisions;

[ApiController]
[Route("api/backoffice")]
public class SubdivisionPaymentsController : ControllerBase
{
    public SubdivisionPaymentsController()
    {
    }

    // TODO: implement
    [HttpGet("subdivisions/{id}/payments")]
    public IActionResult GetSubdivisionPayments(Guid id)
    {
        return StatusCode(501, "Not implemented");
    }

    // TODO: implement
    [HttpPatch("payments/{id}/ap-verification")]
    public IActionResult VerifyPayment(Guid id)
    {
        return StatusCode(501, "Not implemented");
    }
}
