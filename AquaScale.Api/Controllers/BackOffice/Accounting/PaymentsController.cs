using System;
using Microsoft.AspNetCore.Mvc;

namespace AquaScale.Api.Controllers.BackOffice.Accounting;

[ApiController]
[Route("api/backoffice")]
public class PaymentsController : ControllerBase
{
    public PaymentsController()
    {
    }

    // TODO: implement
    [HttpGet("accounting/payments")]
    public IActionResult GetPayments()
    {
        return StatusCode(501, "Not implemented");
    }

    // TODO: implement
    [HttpPatch("payments/{id}/accounting-clearance")]
    public IActionResult ClearPayment(Guid id)
    {
        return StatusCode(501, "Not implemented");
    }
}
