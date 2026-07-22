using Microsoft.AspNetCore.Mvc;
using PayrollMS.Application.Features.Tenant.Commands;
using PayrollMS.Application.Features.Tenant.Queries;
using PayrollMS.Application.Features.Tenant;

namespace PayrollMS.Api.Controllers.v1;

public class CompaniesController : ApiControllerBase
{
    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateCompanyCommand command, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCompanyDetailsCommand command, CancellationToken cancellationToken)
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
        await Mediator.Send(new ActivateCompanyCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        await Mediator.Send(new DeactivateCompanyCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CompanyResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetCompanyByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CompanyResponse>>> List(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new ListCompaniesQuery(), cancellationToken);
        return Ok(result);
    }
}
