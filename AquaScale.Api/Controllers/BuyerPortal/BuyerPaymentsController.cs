using Microsoft.AspNetCore.Mvc;

namespace AquaScale.Api.Controllers.BuyerPortal;

[ApiController]
[Route("api/buyer/payments")]
public class BuyerPaymentsController : ControllerBase
{
    public BuyerPaymentsController()
    {
    }

    // TODO: implement
    [HttpPost]
    public IActionResult CreatePayment()
    {
        return StatusCode(501, "Not implemented");
    }
}
