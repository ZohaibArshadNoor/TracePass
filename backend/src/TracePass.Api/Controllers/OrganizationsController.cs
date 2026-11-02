using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TracePass.Application.Organizations;

namespace TracePass.Api.Controllers;

[ApiController]
[Route("api/organizations")]
[Authorize(Policy = "AdministratorOnly")]
public class OrganizationsController : ControllerBase
{
    private readonly IOrganizationService _organizationService;
    private readonly IValidator<OrganizationRequest> _validator;

    public OrganizationsController(
        IOrganizationService organizationService,
        IValidator<OrganizationRequest> validator)
    {
        _organizationService = organizationService;
        _validator = validator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _organizationService.GetAllAsync());
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var organization =
            await _organizationService.GetByIdAsync(id);

        return organization is null
            ? NotFound(new { message = $"Organization with ID '{id}' was not found." })
            : Ok(organization);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        OrganizationRequest request)
    {
        var validation = await _validator.ValidateAsync(request);

        if (!validation.IsValid)
        {
            return BadRequest(new
            {
                message = "Validation failed.",
                errors = validation.Errors.Select(error => new
                {
                    field = error.PropertyName,
                    message = error.ErrorMessage
                })
            });
        }

        var organization =
            await _organizationService.CreateAsync(request);

        return CreatedAtAction(
            nameof(GetById),
            new { id = organization.Id },
            organization);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        OrganizationRequest request)
    {
        var validation = await _validator.ValidateAsync(request);

        if (!validation.IsValid)
        {
            return BadRequest(new
            {
                message = "Validation failed.",
                errors = validation.Errors.Select(error => new
                {
                    field = error.PropertyName,
                    message = error.ErrorMessage
                })
            });
        }

        var organization =
            await _organizationService.UpdateAsync(id, request);

        return organization is null
            ? NotFound(new { message = $"Organization with ID '{id}' was not found." })
            : Ok(organization);
    }

    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var success =
            await _organizationService.DeactivateAsync(id);

        return success
            ? NoContent()
            : NotFound(new { message = $"Organization with ID '{id}' was not found." });
    }
}
