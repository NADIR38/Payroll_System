using Microsoft.AspNetCore.Mvc;
using PayrollMS.Application.Features.Tenant.Commands;
using PayrollMS.Application.Features.Tenant.Queries;
using PayrollMS.Application.Features.Tenant;

namespace PayrollMS.Api.Controllers.v1;

public class DepartmentsController : ApiControllerBase
{
    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateDepartmentCommand command, CancellationToken cancellationToken)
    {
        if (command.CompanyId == Guid.Empty)
        {
            command = command with { CompanyId = GetCompanyId() };
        }

        var result = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDepartmentDetailsCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return BadRequest("ID in URL path does not match ID in request body.");
        }

        await Mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        await Mediator.Send(new ActivateDepartmentCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        await Mediator.Send(new DeactivateDepartmentCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DepartmentResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetDepartmentByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DepartmentResponse>>> List([FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        if (companyId == Guid.Empty)
        {
            companyId = GetCompanyId();
        }

        var result = await Mediator.Send(new ListDepartmentsQuery(companyId), cancellationToken);
        return Ok(result);
    }
}
