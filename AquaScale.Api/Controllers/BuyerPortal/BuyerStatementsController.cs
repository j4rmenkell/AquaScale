using System;
using Microsoft.AspNetCore.Mvc;

namespace AquaScale.Api.Controllers.BuyerPortal;

[ApiController]
[Route("api/buyer/properties/{id}")]
public class BuyerStatementsController : ControllerBase
{
    public BuyerStatementsController()
    {
    }

    // TODO: implement
    [HttpGet("bills/current")]
    public IActionResult GetCurrentBills(Guid id)
    {
        return StatusCode(501, "Not implemented");
    }

    // TODO: implement
    [HttpGet("consumption")]
    public IActionResult GetConsumption(Guid id)
    {
        return StatusCode(501, "Not implemented");
    }
}
