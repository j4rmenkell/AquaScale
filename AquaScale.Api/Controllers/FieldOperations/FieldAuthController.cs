using Microsoft.AspNetCore.Mvc;

namespace AquaScale.Api.Controllers.FieldOperations;

[ApiController]
[Route("api/field/auth")]
public class FieldAuthController : ControllerBase
{
    public FieldAuthController()
    {
    }

    // TODO: implement
    [HttpPost("login")]
    public IActionResult Login()
    {
        return StatusCode(501, "Not implemented");
    }
}
