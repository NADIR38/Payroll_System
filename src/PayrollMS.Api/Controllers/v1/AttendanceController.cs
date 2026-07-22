using Microsoft.AspNetCore.Mvc;
using PayrollMS.Application.Features.Attendance;
using PayrollMS.Application.Features.Attendance.Commands.SyncAttendance;
using PayrollMS.Application.Features.Attendance.Commands.SyncLeave;

namespace PayrollMS.Api.Controllers.v1;

[Route("api/v1")]
public class AttendanceController : ApiControllerBase
{
    [HttpPost("attendance/sync")]
    public async Task<ActionResult<AttendanceSummaryResponse>> SyncAttendance(SyncAttendanceCommand command)
    {
        if (command.CompanyId == Guid.Empty)
        {
            command = command with { CompanyId = GetCompanyId() };
        }

        var result = await Mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("leave/sync")]
    public async Task<ActionResult<LeaveSummaryResponse>> SyncLeave(SyncLeaveCommand command)
    {
        if (command.CompanyId == Guid.Empty)
        {
            command = command with { CompanyId = GetCompanyId() };
        }

        var result = await Mediator.Send(command);
        return Ok(result);
    }
}
