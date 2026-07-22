using Microsoft.AspNetCore.Mvc;
using PayrollMS.Application.Features.Employee.Commands.AddEmployeeBankAccount;
using PayrollMS.Application.Features.Employee.Commands.SetPrimaryBankAccount;
using PayrollMS.Application.Features.Employee.Commands.SyncEmployee;
using PayrollMS.Application.Features.Employee.Commands.TerminateEmployee;
using PayrollMS.Application.Features.Employee.Commands.UpdateEmployeeProfile;
using PayrollMS.Application.Features.Employee.Queries.GetEmployeeProfileById;
using PayrollMS.Application.Features.Employee.Queries.GetEmployeeProfileHistory;
using PayrollMS.Application.Features.Employee.Queries.GetEmployeeProfiles;
using PayrollMS.Application.Features.Employee;

namespace PayrollMS.Api.Controllers.v1;

public class EmployeesController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EmployeeProfileResponse>>> GetEmployees(
        [FromQuery] string? searchTerm,
        [FromQuery] string? statusFilter,
        [FromQuery] Guid? branchId,
        [FromQuery] Guid? departmentId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50)
    {
        var companyId = GetCompanyId();
        var query = new GetEmployeeProfilesQuery(
            companyId,
            searchTerm,
            statusFilter,
            branchId,
            departmentId,
            pageNumber,
            pageSize);

        var result = await Mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EmployeeProfileResponse>> GetEmployeeById(Guid id)
    {
        var result = await Mediator.Send(new GetEmployeeProfileByIdQuery(id));
        return Ok(result);
    }

    [HttpGet("{id:guid}/history")]
    public async Task<ActionResult<IReadOnlyList<EmployeeProfileHistoryResponse>>> GetEmployeeHistory(Guid id)
    {
        var result = await Mediator.Send(new GetEmployeeProfileHistoryQuery(id));
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> SyncEmployee(SyncEmployeeCommand command)
    {
        if (command.CompanyId == Guid.Empty)
        {
            command = command with { CompanyId = GetCompanyId() };
        }

        var result = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetEmployeeById), new { id = result.ProfileId }, result.ProfileId);
    }

    [HttpPut("{id:guid}/profile")]
    public async Task<ActionResult> UpdateProfile(Guid id, UpdateEmployeeProfileCommand command)
    {
        if (id != command.Id)
            return BadRequest("ID mismatch in URL and body.");

        await Mediator.Send(command);
        return NoContent();
    }

    [HttpPost("{id:guid}/terminate")]
    public async Task<ActionResult> TerminateEmployee(Guid id, TerminateEmployeeCommand command)
    {
        if (id != command.Id)
            return BadRequest("ID mismatch in URL and body.");

        await Mediator.Send(command);
        return NoContent();
    }

    [HttpPost("{id:guid}/bank-accounts")]
    public async Task<ActionResult<Guid>> AddBankAccount(Guid id, AddEmployeeBankAccountCommand command)
    {
        if (id != command.EmployeePayrollProfileId)
            return BadRequest("ID mismatch in URL and body.");

        var accountId = await Mediator.Send(command);
        return Ok(accountId);
    }

    [HttpPut("{id:guid}/bank-accounts/{accountId:guid}/primary")]
    public async Task<ActionResult> SetPrimaryBankAccount(Guid id, Guid accountId)
    {
        var command = new SetPrimaryBankAccountCommand(id, accountId);
        await Mediator.Send(command);
        return NoContent();
    }
}
