namespace TracePass.Application.Common.Interfaces;

public interface ICurrentUser
{
    string? UserId { get; }

    bool IsAuthenticated { get; }

    bool IsAdministrator { get; }

    Task<Guid?> GetOrganizationIdAsync(
        CancellationToken cancellationToken = default);
}