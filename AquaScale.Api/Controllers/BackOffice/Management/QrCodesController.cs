using Microsoft.AspNetCore.Mvc;

namespace AquaScale.Api.Controllers.BackOffice.Management;

[ApiController]
[Route("api/backoffice")]
public class QrCodesController : ControllerBase
{
    public QrCodesController()
    {
    }

    // TODO: implement
    [HttpGet("qr-search")]
    public IActionResult SearchQr()
    {
        return StatusCode(501, "Not implemented");
    }

    // TODO: implement
    [HttpPost("qr-batch")]
    public IActionResult CreateQrBatch()
    {
        return StatusCode(501, "Not implemented");
    }
}
