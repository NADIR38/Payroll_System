using Microsoft.AspNetCore.Mvc;
using PayrollMS.Application.Features.SalaryComponents;
using PayrollMS.Application.Features.SalaryComponents.Commands.CreateSalaryComponent;
using PayrollMS.Application.Features.SalaryComponents.Commands.DeactivateSalaryComponent;
using PayrollMS.Application.Features.SalaryComponents.Commands.SeedPredefinedSalaryComponents;
using PayrollMS.Application.Features.SalaryComponents.Commands.UpdateSalaryComponent;
using PayrollMS.Application.Features.SalaryComponents.Queries.GetSalaryComponentById;
using PayrollMS.Application.Features.SalaryComponents.Queries.GetSalaryComponents;

namespace PayrollMS.Api.Controllers.v1;

[Route("api/v1/salary-components")]
public class SalaryComponentsController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SalaryComponentResponse>>> GetSalaryComponents(
        [FromQuery] string? type = null,
        [FromQuery] bool isActiveOnly = true)
    {
        var companyId = GetCompanyId();
        var query = new GetSalaryComponentsQuery(companyId, type, isActiveOnly);
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SalaryComponentResponse>> GetSalaryComponentById(Guid id)
    {
        var companyId = GetCompanyId();
        return await Mediator.Send(new GetSalaryComponentByIdQuery(id, companyId));
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> CreateSalaryComponent(CreateSalaryComponentCommand command)
    {
        if (command.CompanyId == Guid.Empty)
        {
            command = command with { CompanyId = GetCompanyId() };
        }

        var id = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetSalaryComponentById), new { id }, id);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> UpdateSalaryComponent(Guid id, UpdateSalaryComponentCommand command)
    {
        if (id != command.ComponentId)
            return BadRequest("ID mismatch in URL and body.");

        if (command.CompanyId == Guid.Empty)
        {
            command = command with { CompanyId = GetCompanyId() };
        }

        await Mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeactivateSalaryComponent(Guid id)
    {
        var companyId = GetCompanyId();
        var command = new DeactivateSalaryComponentCommand(id, companyId);
        await Mediator.Send(command);
        return NoContent();
    }

    [HttpPost("seed")]
    public async Task<ActionResult<PredefinedComponentSeedResult>> SeedPredefinedComponents()
    {
        var companyId = GetCompanyId();
        var command = new SeedPredefinedSalaryComponentsCommand(companyId);
        var result = await Mediator.Send(command);
        return Ok(result);
    }
}
