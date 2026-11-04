using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TracePass.Application.Common.Paging;
using TracePass.Application.Materials;

namespace TracePass.Api.Controllers;

[ApiController]
[Route("api/materials")]
[Authorize]
public class MaterialsController : ControllerBase
{
    private readonly IMaterialService _materialService;
    private readonly IValidator<CreateMaterialRequest> _createValidator;
    private readonly IValidator<UpdateMaterialRequest> _updateValidator;

    public MaterialsController(
        IMaterialService materialService,
        IValidator<CreateMaterialRequest> createValidator,
        IValidator<UpdateMaterialRequest> updateValidator)
    {
        _materialService = materialService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] PagedRequest request,
        [FromQuery] string? category = null)
    {
        var result = await _materialService.GetPagedAsync(request, category);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var material = await _materialService.GetByIdAsync(id);
        return material == null
            ? NotFound(new { message = $"Material with ID '{id}' was not found." })
            : Ok(material);
    }

    [HttpPost]
    [Authorize(Policy = "AdministratorOnly")]
    public async Task<IActionResult> Create([FromBody] CreateMaterialRequest request)
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

        var material = await _materialService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = material.Id }, material);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "AdministratorOnly")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMaterialRequest request)
    {
        var validation = await _updateValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return BadRequest(new
            {
                message = "Validation failed.",
                errors = validation.Errors.Select(e => new { field = e.PropertyName, message = e.ErrorMessage })
            });
        }

        var material = await _materialService.UpdateAsync(id, request);
        return material == null
            ? NotFound(new { message = $"Material with ID '{id}' was not found." })
            : Ok(material);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdministratorOnly")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _materialService.DeleteAsync(id);
        return success
            ? NoContent()
            : NotFound(new { message = $"Material with ID '{id}' was not found." });
    }
}
