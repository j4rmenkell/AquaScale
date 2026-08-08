using Microsoft.AspNetCore.Mvc;

namespace AquaScale.Api.Controllers.BackOffice.Accounting;

[ApiController]
[Route("api/backoffice/billing-cycles")]
public class BillingController : ControllerBase
{
    public BillingController()
    {
    }

    // TODO: implement
    [HttpPost]
    public IActionResult TriggerBillingCycle()
    {
        return StatusCode(501, "Not implemented");
    }
}
