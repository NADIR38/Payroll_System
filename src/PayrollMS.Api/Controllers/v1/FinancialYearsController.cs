using Microsoft.AspNetCore.Mvc;
using PayrollMS.Application.Features.Tenant.Commands;
using PayrollMS.Application.Features.Tenant.Queries;
using PayrollMS.Application.Features.Tenant;

namespace PayrollMS.Api.Controllers.v1;

[Route("api/v1/financial-years")]
public class FinancialYearsController : ApiControllerBase
{
    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateFinancialYearCommand command, CancellationToken cancellationToken)
    {
        if (command.CompanyId == Guid.Empty)
        {
            command = command with { CompanyId = GetCompanyId() };
        }

        var result = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result }, result);
    }

    [HttpPost("{id:guid}/mark-current")]
    public async Task<IActionResult> MarkCurrent(Guid id, CancellationToken cancellationToken)
    {
        await Mediator.Send(new MarkFinancialYearCurrentCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<FinancialYearResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetFinancialYearByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<FinancialYearResponse>>> List([FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        if (companyId == Guid.Empty)
        {
            companyId = GetCompanyId();
        }

        var result = await Mediator.Send(new ListFinancialYearsQuery(companyId), cancellationToken);
        return Ok(result);
    }
}
