using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TracePass.Application.Users;

namespace TracePass.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Policy = "AdministratorOnly")]
public class UsersController : ControllerBase
{
    private readonly IUserManagementService _userService;
    private readonly IValidator<CreateUserRequest> _validator;

    public UsersController(
        IUserManagementService userService,
        IValidator<CreateUserRequest> validator)
    {
        _userService = userService;
        _validator = validator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _userService.GetAllAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var user = await _userService.GetByIdAsync(id);

        return user is null
            ? NotFound(new { message = $"User with ID '{id}' was not found." })
            : Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateUserRequest request)
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

        var result = await _userService.CreateAsync(request);

        if (result.User is null)
        {
            return BadRequest(new { message = result.Error });
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.User.Id },
            result.User);
    }

    [HttpPatch("{id}/activate")]
    public async Task<IActionResult> Activate(string id)
    {
        var success =
            await _userService.SetActiveStatusAsync(id, true);

        return success
            ? NoContent()
            : NotFound(new { message = $"User with ID '{id}' was not found." });
    }

    [HttpPatch("{id}/deactivate")]
    public async Task<IActionResult> Deactivate(string id)
    {
        var success =
            await _userService.SetActiveStatusAsync(id, false);

        return success
            ? NoContent()
            : NotFound(new { message = $"User with ID '{id}' was not found." });
    }
}
