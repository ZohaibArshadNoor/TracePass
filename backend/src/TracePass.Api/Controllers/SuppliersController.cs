using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TracePass.Application.Common.Paging;
using TracePass.Application.Suppliers;

namespace TracePass.Api.Controllers;

[ApiController]
[Route("api/suppliers")]
[Authorize]
public class SuppliersController : ControllerBase
{
    private readonly ISupplierService _supplierService;
    private readonly IValidator<CreateSupplierRequest> _createValidator;
    private readonly IValidator<UpdateSupplierProfileRequest> _profileValidator;

    public SuppliersController(
        ISupplierService supplierService,
        IValidator<CreateSupplierRequest> createValidator,
        IValidator<UpdateSupplierProfileRequest> profileValidator)
    {
        _supplierService = supplierService;
        _createValidator = createValidator;
        _profileValidator = profileValidator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PagedRequest request)
    {
        var result = await _supplierService.GetPagedAsync(request);
        return Ok(result);
    }

    [HttpGet("{organizationId:guid}")]
    public async Task<IActionResult> GetByOrganizationId(Guid organizationId)
    {
        var supplier = await _supplierService.GetByOrganizationIdAsync(organizationId);
        return supplier == null
            ? NotFound(new { message = $"Supplier with Organization ID '{organizationId}' was not found." })
            : Ok(supplier);
    }

    [HttpPost]
    [Authorize(Policy = "AdministratorOnly")]
    public async Task<IActionResult> Create([FromBody] CreateSupplierRequest request)
    {
        var validation = await _createValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return BadRequest(new
            {
                message = "Validation failed.",
                errors = validation.Errors.Select(e => new { field = e.PropertyName, message = e.ErrorMessage })
            });
        }

        var supplier = await _supplierService.CreateSupplierAsync(request);
        return CreatedAtAction(nameof(GetByOrganizationId), new { organizationId = supplier.OrganizationId }, supplier);
    }

    [HttpPut("{organizationId:guid}/profile")]
    [Authorize(Policy = "AdministratorOnly")]
    public async Task<IActionResult> UpdateProfile(
        Guid organizationId,
        [FromBody] UpdateSupplierProfileRequest request)
    {
        var validation = await _profileValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return BadRequest(new
            {
                message = "Validation failed.",
                errors = validation.Errors.Select(e => new { field = e.PropertyName, message = e.ErrorMessage })
            });
        }

        var updated = await _supplierService.UpdateProfileAsync(organizationId, request);
        return updated == null
            ? NotFound(new { message = $"Supplier with Organization ID '{organizationId}' was not found." })
            : Ok(updated);
    }
}
