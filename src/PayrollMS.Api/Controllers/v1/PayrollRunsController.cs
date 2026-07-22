using Microsoft.AspNetCore.Mvc;
using PayrollMS.Application.Features.PayrollRuns;
using PayrollMS.Application.Features.PayrollRuns.Commands.ApprovePayrollRun;
using PayrollMS.Application.Features.PayrollRuns.Commands.CreatePayrollRun;
using PayrollMS.Application.Features.PayrollRuns.Commands.OverrideComponent;
using PayrollMS.Application.Features.PayrollRuns.Commands.RejectPayrollRun;
using PayrollMS.Application.Features.PayrollRuns.Queries;
using PayrollMS.Domain.Enums;

namespace PayrollMS.Api.Controllers.v1;

[Route("api/v1/payroll/runs")]
public class PayrollRunsController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedPayrollRunsResponse>> GetPayrollRuns(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] PayrollRunStatus? status = null,
        [FromQuery] int? periodYear = null,
        [FromQuery] int? periodMonth = null)
    {
        var companyId = GetCompanyId();
        var query = new GetPayrollRunsQuery(companyId, page, pageSize, status, periodYear, periodMonth);
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PayrollRunResponse>> GetPayrollRunById(Guid id)
    {
        return await Mediator.Send(new GetPayrollRunByIdQuery(id));
    }

    [HttpPost]
    public async Task<ActionResult<PayrollRunCreatedResult>> CreatePayrollRun(CreatePayrollRunCommand command)
    {
        if (command.CompanyId == Guid.Empty)
        {
            command = command with { CompanyId = GetCompanyId() };
        }

        var result = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetPayrollRunById), new { id = result.PayrollRunId }, result);
    }

    [HttpGet("{id:guid}/entries")]
    public async Task<ActionResult<PagedPayrollEntriesResponse>> GetPayrollEntries(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] PayrollEntryStatus? status = null,
        [FromQuery] string? search = null)
    {
        var query = new GetPayrollEntriesQuery(id, page, pageSize, status, search);
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/entries/{entryId:guid}/components/{componentId:guid}")]
    public async Task<ActionResult<PayrollEntryComponentResponse>> OverrideComponent(
        Guid id,
        Guid entryId,
        Guid componentId,
        [FromBody] OverrideComponentRequest request)
    {
        var command = new OverrideComponentCommand(
            id, entryId, componentId, request.OverrideAmount, request.OverriddenBy, request.Reason);

        var result = await Mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<ActionResult<PayrollRunResponse>> ApprovePayrollRun(Guid id, [FromBody] ApproveRequest request)
    {
        var companyId = GetCompanyId();
        var command = new ApprovePayrollRunCommand(
            id, companyId, request.ActorUserId, request.ActorName, request.ActorRole, request.Comments);

        var result = await Mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<ActionResult<PayrollRunResponse>> RejectPayrollRun(Guid id, [FromBody] RejectRequest request)
    {
        var companyId = GetCompanyId();
        var command = new RejectPayrollRunCommand(
            id, companyId, request.ActorUserId, request.ActorName, request.ActorRole, request.Comments);

        var result = await Mediator.Send(command);
        return Ok(result);
    }
}

public record OverrideComponentRequest(decimal OverrideAmount, string OverriddenBy, string Reason);
public record ApproveRequest(string ActorUserId, string ActorName, string ActorRole, string? Comments);
public record RejectRequest(string ActorUserId, string ActorName, string ActorRole, string Comments);
