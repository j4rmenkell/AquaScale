using Microsoft.AspNetCore.Mvc;

namespace AquaScale.Api.Controllers.BuyerPortal;

[ApiController]
[Route("api/buyer/service-requests")]
public class BuyerServiceRequestsController : ControllerBase
{
    public BuyerServiceRequestsController()
    {
    }

    // TODO: implement
    [HttpPost]
    public IActionResult CreateServiceRequest()
    {
        return StatusCode(501, "Not implemented");
    }
}
