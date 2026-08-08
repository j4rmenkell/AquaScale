using Microsoft.AspNetCore.Mvc;

namespace AquaScale.Api.Controllers.FieldOperations;

[ApiController]
[Route("api/field/assignments")]
public class AssignmentsController : ControllerBase
{
    public AssignmentsController()
    {
    }

    // TODO: implement
    [HttpGet]
    public IActionResult GetAssignments()
    {
        return StatusCode(501, "Not implemented");
    }
}
