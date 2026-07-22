using MediatR;

namespace PayrollMS.Application.Features.Employee.Commands.TerminateEmployee;

public sealed record TerminateEmployeeCommand(
    Guid Id,
    string ChangedBy) : IRequest;
