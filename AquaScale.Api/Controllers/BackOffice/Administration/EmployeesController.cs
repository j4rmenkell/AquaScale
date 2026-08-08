using System;
using Microsoft.AspNetCore.Mvc;

namespace AquaScale.Api.Controllers.BackOffice.Administration;

[ApiController]
[Route("api/backoffice/employees")]
public class EmployeesController : ControllerBase
{
    public EmployeesController()
    {
    }

    // TODO: implement
    [HttpGet]
    public IActionResult GetEmployees()
    {
        return StatusCode(501, "Not implemented");
    }

    // TODO: implement
    [HttpPut("{id}/assignments")]
    public IActionResult UpdateAssignments(Guid id)
    {
        return StatusCode(501, "Not implemented");
    }
}
