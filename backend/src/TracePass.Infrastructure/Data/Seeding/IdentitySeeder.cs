
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using TracePass.Infrastructure.Identity;

namespace TracePass.Infrastructure.Data.Seeding;

public static class IdentitySeeder
{
    public static async Task SeedAsync(
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration)
    {
        string[] roles =
        [
            TracePassRoles.Administrator,
            TracePassRoles.Manufacturer,
            TracePassRoles.Supplier,
            TracePassRoles.Distributor,
            TracePassRoles.Buyer,
            TracePassRoles.Auditor
        ];

        foreach (var role in roles)
        {
            if (await roleManager.RoleExistsAsync(role))
            {
                continue;
            }

            var result = await roleManager.CreateAsync(
                new IdentityRole(role));

            if (!result.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    result.Errors.Select(error => error.Description));

                throw new InvalidOperationException(
                    $"Could not create role '{role}': {errors}");
            }
        }

        var email = configuration["InitialAdmin:Email"];
        var password = configuration["InitialAdmin:Password"];
        var firstName = configuration["InitialAdmin:FirstName"];
        var lastName = configuration["InitialAdmin:LastName"];

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(firstName) ||
            string.IsNullOrWhiteSpace(lastName))
        {
            throw new InvalidOperationException(
                "Initial administrator settings are missing. Configure " +
                "InitialAdmin:Email, Password, FirstName, and LastName " +
                "using development user-secrets or environment variables.");
        }

        var admin = await userManager.FindByEmailAsync(email);

        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = firstName,
                LastName = lastName,
                IsActive = true
            };

            var createResult = await userManager.CreateAsync(
                admin,
                password);

            if (!createResult.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    createResult.Errors.Select(error => error.Description));

                throw new InvalidOperationException(
                    $"Could not create initial administrator: {errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(
                admin,
                TracePassRoles.Administrator))
        {
            var roleResult = await userManager.AddToRoleAsync(
                admin,
                TracePassRoles.Administrator);

            if (!roleResult.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    roleResult.Errors.Select(error => error.Description));

                throw new InvalidOperationException(
                    $"Could not assign Administrator role: {errors}");
            }
        }
    }
}
