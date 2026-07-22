using Microsoft.AspNetCore.Mvc;
using PayrollMS.Application.Features.Approvals;
using PayrollMS.Application.Features.Approvals.Commands.CreateWorkflowTemplate;
using PayrollMS.Application.Features.Approvals.Queries;

namespace PayrollMS.Api.Controllers.v1;

[Route("api/v1/approvals")]
public class ApprovalsController : ApiControllerBase
{
    [HttpGet("pending")]
    public async Task<ActionResult<IReadOnlyList<PendingApprovalItemResponse>>> GetPendingApprovals(
        [FromQuery] string userRole = "HRManager")
    {
        var companyId = GetCompanyId();
        var query = new GetPendingApprovalsQuery(companyId, userRole);
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    [HttpPost("workflow-templates")]
    public async Task<ActionResult<ApprovalWorkflowTemplateResponse>> CreateWorkflowTemplate(
        CreateWorkflowTemplateCommand command)
    {
        if (command.CompanyId == Guid.Empty)
        {
            command = command with { CompanyId = GetCompanyId() };
        }

        var result = await Mediator.Send(command);
        return Ok(result);
    }
}
