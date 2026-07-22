using Microsoft.AspNetCore.Mvc;
using PayrollMS.Application.Features.Tenant.Commands;
using PayrollMS.Application.Features.Tenant.Queries;
using PayrollMS.Application.Features.Tenant;

namespace PayrollMS.Api.Controllers.v1;

[Route("api/v1/payroll-calendars")]
public class PayrollCalendarsController : ApiControllerBase
{
    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] CreatePayrollCalendarCommand command, CancellationToken cancellationToken)
    {
        if (command.CompanyId == Guid.Empty)
        {
            command = command with { CompanyId = GetCompanyId() };
        }

        var result = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result }, result);
    }

    [HttpPost("{id:guid}/freeze")]
    public async Task<IActionResult> Freeze(Guid id, CancellationToken cancellationToken)
    {
        await Mediator.Send(new FreezePayrollCalendarCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/close")]
    public async Task<IActionResult> Close(Guid id, CancellationToken cancellationToken)
    {
        await Mediator.Send(new ClosePayrollCalendarCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/reopen")]
    public async Task<IActionResult> Reopen(Guid id, CancellationToken cancellationToken)
    {
        await Mediator.Send(new ReopenPayrollCalendarCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PayrollCalendarResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetPayrollCalendarByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PayrollCalendarResponse>>> List(
        [FromQuery] Guid companyId,
        [FromQuery] Guid financialYearId,
        CancellationToken cancellationToken)
    {
        if (companyId == Guid.Empty)
        {
            companyId = GetCompanyId();
        }

        var result = await Mediator.Send(new ListPayrollCalendarsQuery(companyId, financialYearId), cancellationToken);
        return Ok(result);
    }
}
