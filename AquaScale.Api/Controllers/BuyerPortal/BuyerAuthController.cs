using Microsoft.AspNetCore.Mvc;

namespace AquaScale.Api.Controllers.BuyerPortal;

[ApiController]
[Route("api/buyer/auth")]
public class BuyerAuthController : ControllerBase
{
    public BuyerAuthController()
    {
    }

    // TODO: implement
    [HttpPost("login")]
    public IActionResult Login()
    {
        return StatusCode(501, "Not implemented");
    }
}
