using System;
using System.Collections.Generic;
using System.Text;

namespace TracePass.Application.Authentication;

public class CurrentUserResponse
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public Guid? OrganizationId { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();
}