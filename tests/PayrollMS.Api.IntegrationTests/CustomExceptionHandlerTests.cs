using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using PayrollMS.Application.Common.Exceptions;
using PayrollMS.Application.Features.Tenant.Commands;
using PayrollMS.Application.Features.Tenant.Queries;
using PayrollMS.Domain.Exceptions;
using Xunit;

namespace PayrollMS.Api.IntegrationTests;

public class CustomExceptionHandlerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public CustomExceptionHandlerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetCompany_ShouldReturnNotFoundProblemDetails_WhenNotFoundExceptionIsThrown()
    {
        // Arrange
        var mockMediator = Substitute.For<ISender>();
        var companyId = Guid.NewGuid();

        mockMediator.Send(Arg.Any<GetCompanyByIdQuery>(), Arg.Any<CancellationToken>())
            .Throws(new NotFoundException($"Company with ID '{companyId}' was not found."));

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ISender));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                services.AddScoped(_ => mockMediator);
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync($"/api/v1/companies/{companyId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Title.Should().Be("Not Found");
        problemDetails.Detail.Should().Contain("was not found");
        problemDetails.Status.Should().Be(404);
    }

    [Fact]
    public async Task CreateCompany_ShouldReturnBadRequestProblemDetails_WhenValidationExceptionIsThrown()
    {
        // Arrange
        var mockMediator = Substitute.For<ISender>();
        var failures = new[]
        {
            new ValidationFailure("ContactEmail", "Email is invalid.")
        };

        mockMediator.Send(Arg.Any<CreateCompanyCommand>(), Arg.Any<CancellationToken>())
            .Throws(new ValidationException(failures));

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ISender));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                services.AddScoped(_ => mockMediator);
            });
        }).CreateClient();

        var command = new CreateCompanyCommand("Acme", "ACME", null, null, "bad-email", null);

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/companies", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problemDetails = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Title.Should().Be("Bad Request");
        problemDetails.Errors.Should().ContainKey("ContactEmail");
        problemDetails.Errors["ContactEmail"].Should().Contain("Email is invalid.");
        problemDetails.Status.Should().Be(400);
    }

    [Fact]
    public async Task CreateCompany_ShouldReturnUnprocessableEntity_WhenBusinessRuleViolationExceptionIsThrown()
    {
        // Arrange
        var mockMediator = Substitute.For<ISender>();
        mockMediator.Send(Arg.Any<CreateCompanyCommand>(), Arg.Any<CancellationToken>())
            .Throws(new BusinessRuleViolationException("DUPLICATE_COMPANY_CODE", "Company code already exists."));

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ISender));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                services.AddScoped(_ => mockMediator);
            });
        }).CreateClient();

        var command = new CreateCompanyCommand("Acme", "ACME", null, null, null, null);

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/companies", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Title.Should().Be("Business Rule Violation");
        problemDetails.Status.Should().Be(422);
    }
}
