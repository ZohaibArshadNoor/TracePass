namespace TracePass.Application.Users;

public interface IUserManagementService
{
    Task<IReadOnlyList<UserResponse>> GetAllAsync();

    Task<UserResponse?> GetByIdAsync(string id);

    Task<(UserResponse? User, string? Error)> CreateAsync(
        CreateUserRequest request);

    Task<bool> SetActiveStatusAsync(string id, bool isActive);
}
