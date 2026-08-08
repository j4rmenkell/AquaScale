using Microsoft.AspNetCore.Mvc;

namespace AquaScale.Api.Controllers.BackOffice.Administration;

[ApiController]
[Route("api/backoffice/audit-logs")]
public class AuditLogsController : ControllerBase
{
    public AuditLogsController()
    {
    }

    // TODO: implement
    [HttpGet]
    public IActionResult GetAuditLogs()
    {
        return StatusCode(501, "Not implemented");
    }
}
