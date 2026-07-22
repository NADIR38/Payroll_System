using Microsoft.AspNetCore.Mvc;
using PayrollMS.Application.Features.SalaryStructures;
using PayrollMS.Application.Features.SalaryStructures.Commands.AddComponentToStructure;
using PayrollMS.Application.Features.SalaryStructures.Commands.CreateSalaryStructure;
using PayrollMS.Application.Features.SalaryStructures.Commands.DeactivateSalaryStructure;
using PayrollMS.Application.Features.SalaryStructures.Commands.RemoveComponentFromStructure;
using PayrollMS.Application.Features.SalaryStructures.Commands.UpdateComponentInStructure;
using PayrollMS.Application.Features.SalaryStructures.Commands.SetAllowanceRule;
using PayrollMS.Application.Features.SalaryStructures.Commands.SetDeductionRule;
using PayrollMS.Application.Features.SalaryStructures.Queries.GetSalaryStructureById;
using PayrollMS.Application.Features.SalaryStructures.Queries.GetSalaryStructures;
using PayrollMS.Application.Features.SalaryStructures.Queries.ValidateFormula;

namespace PayrollMS.Api.Controllers.v1;

[Route("api/v1/salary-structures")]
public class SalaryStructuresController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SalaryStructureResponse>>> GetSalaryStructures(
        [FromQuery] bool isActiveOnly = true)
    {
        var companyId = GetCompanyId();
        var query = new GetSalaryStructuresQuery(companyId, isActiveOnly);
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SalaryStructureResponse>> GetSalaryStructureById(Guid id)
    {
        var companyId = GetCompanyId();
        return await Mediator.Send(new GetSalaryStructureByIdQuery(id, companyId));
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> CreateSalaryStructure(CreateSalaryStructureCommand command)
    {
        if (command.CompanyId == Guid.Empty)
        {
            command = command with { CompanyId = GetCompanyId() };
        }

        var id = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetSalaryStructureById), new { id }, id);
    }

    [HttpPost("{id:guid}/components")]
    public async Task<ActionResult<Guid>> AddComponentToStructure(Guid id, AddComponentToStructureCommand command)
    {
        if (id != command.SalaryStructureId)
            return BadRequest("ID mismatch in URL and body.");

        if (command.CompanyId == Guid.Empty)
        {
            command = command with { CompanyId = GetCompanyId() };
        }

        var componentId = await Mediator.Send(command);
        return Ok(componentId);
    }

    [HttpPut("{id:guid}/components/{componentId:guid}")]
    public async Task<ActionResult> UpdateComponentInStructure(Guid id, Guid componentId, UpdateComponentInStructureCommand command)
    {
        if (id != command.SalaryStructureId || componentId != command.StructureComponentId)
            return BadRequest("ID mismatch in URL and body.");

        if (command.CompanyId == Guid.Empty)
        {
            command = command with { CompanyId = GetCompanyId() };
        }

        await Mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id:guid}/components/{componentId:guid}")]
    public async Task<ActionResult> RemoveComponentFromStructure(Guid id, Guid componentId)
    {
        var companyId = GetCompanyId();
        var command = new RemoveComponentFromStructureCommand(id, companyId, componentId);
        await Mediator.Send(command);
        return NoContent();
    }

    [HttpPut("{id:guid}/components/{componentId:guid}/allowance-rule")]
    public async Task<ActionResult<Guid>> SetAllowanceRule(Guid id, Guid componentId, SetAllowanceRuleCommand command)
    {
        if (id != command.SalaryStructureId || componentId != command.StructureComponentId)
            return BadRequest("ID mismatch in URL and body.");

        if (command.CompanyId == Guid.Empty)
        {
            command = command with { CompanyId = GetCompanyId() };
        }

        var ruleId = await Mediator.Send(command);
        return Ok(ruleId);
    }

    [HttpPut("{id:guid}/components/{componentId:guid}/deduction-rule")]
    public async Task<ActionResult<Guid>> SetDeductionRule(Guid id, Guid componentId, SetDeductionRuleCommand command)
    {
        if (id != command.SalaryStructureId || componentId != command.StructureComponentId)
            return BadRequest("ID mismatch in URL and body.");

        if (command.CompanyId == Guid.Empty)
        {
            command = command with { CompanyId = GetCompanyId() };
        }

        var ruleId = await Mediator.Send(command);
        return Ok(ruleId);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeactivateSalaryStructure(Guid id)
    {
        var companyId = GetCompanyId();
        var command = new DeactivateSalaryStructureCommand(id, companyId);
        await Mediator.Send(command);
        return NoContent();
    }

    [HttpPost("validate-formula")]
    public async Task<ActionResult<FormulaValidationResult>> ValidateFormula(ValidateFormulaQuery query)
    {
        // This is a stateless operation that doesn't need company ID context
        var result = await Mediator.Send(query);
        return Ok(result);
    }
}
