using LoanHub.Events;
using MediatR;

namespace LoanHub.Accounting.Application.Commands.ProcessLoanCreated;

public sealed record ProcessLoanCreatedCommand(LoanCreatedEvent IntegrationEvent) : IRequest;
