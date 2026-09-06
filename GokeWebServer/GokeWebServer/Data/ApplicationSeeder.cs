using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Goke.Core.Extensions;
using Goke.Core.Options;

namespace GokeWebServer.Data;

public partial class ApplicationSeeder
{
    public static async Task SeedAllAsync(IServiceProvider sp)
    {
        var context = sp.GetRequiredService<ApplicationDbContext>();

        //reset the database
        await context.Database.EnsureDeletedAsync();

        //migrate the database
        await context.Database.MigrateAsync();

        //Seed Roles
        await AddRolesAsync(sp);

        //Seed Users
        await AddUsersAsync(sp);

        //Seed other data
        await SeedDataAsync(context);


        await context.SaveChangesAsync();
    }

    private static async Task AddRolesAsync(IServiceProvider sp)
    {
        var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();
        var configuration = sp.GetRequiredService<IConfiguration>();

        await AddRolesAsync(roleManager, configuration);
    }

    private static async Task AddUsersAsync(IServiceProvider sp)
    {
        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
        var configuration = sp.GetRequiredService<IConfiguration>();

        var seeded = await AddUsersAsync(userManager, configuration);
        if (!seeded)
        {
            await EnsureDevelopmentUsersAsync(userManager);
        }
    }

    private static async Task EnsureDevelopmentUsersAsync(UserManager<ApplicationUser> userManager)
    {
        await EnsureDevelopmentUserAsync(userManager, "admin@gok.loc", "admin@gok.loc", "Administrators", "Pass123$");
        await EnsureDevelopmentUserAsync(userManager, "officer@gok.loc", "officer@gok.loc", "Officers", "Pass123$");
        await EnsureDevelopmentUserAsync(userManager, "user@gok.loc", "user@gok.loc", "Users", "Pass123$");
    }

    private static async Task EnsureDevelopmentUserAsync(
        UserManager<ApplicationUser> userManager,
        string userName,
        string email,
        string role,
        string password)
    {
        var existingUser = await userManager.FindByNameAsync(userName);
        if (existingUser != null)
        {
            if (!await userManager.IsInRoleAsync(existingUser, role))
            {
                await userManager.AddToRoleAsync(existingUser, role);
            }

            return;
        }

        var user = new ApplicationUser
        {
            UserName = userName,
            Email = email,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, role);
        }
    }

    private static string GetPasswordToUse(string? password)
    {
        return string.IsNullOrEmpty(password)
            ? string.GeneratePassword(20)
            : password;
    }

    private static async Task SeedDataAsync(ApplicationDbContext context)
    {

    }

    public static async Task AddRolesAsync(RoleManager<IdentityRole> roleManager, IConfiguration configuration)
    {
        var rolesSection = configuration.GetSection("SeedRoles");
        var rolesFromConfig = rolesSection.Get<string>();

        var roles = rolesFromConfig is { Length: > 0 }
            ? rolesFromConfig.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
            : new List<string> { "Administrators", "Officers", "Users" };

        await AddRolesAsync(roleManager, roles);
    }

    public static async Task AddRolesAsync(RoleManager<IdentityRole> roleManager, List<string> roles)
    {
        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }

    public static async Task<bool> AddUsersAsync(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration)
    {
        var usersSection = configuration.GetSection("SeedUsers");
        var usersFromConfig = usersSection.Get<SeedUserOptions[]>();

        return await AddUsersAsync(userManager, usersFromConfig);
    }

    public static async Task<bool> AddUsersAsync(UserManager<ApplicationUser> userManager, SeedUserOptions[]? seedUsers)
    {
        if (seedUsers == null || seedUsers.Length == 0)
        {
            return false;
        }

        foreach (var userOptions in seedUsers)
        {
            var existingUser = await userManager.FindByNameAsync(userOptions.UserName);
            if (existingUser != null)
            {
                continue;
            }

            var user = new ApplicationUser
            {
                UserName = userOptions.UserName,
                Email = userOptions.Email,
                EmailConfirmed = true
            };

            var passwordToUse = GetPasswordToUse(userOptions.Password);

            var result = await userManager.CreateAsync(user, passwordToUse);
            if (result.Succeeded && !string.IsNullOrEmpty(userOptions.Roles))
            {
                var roles = userOptions.Roles.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                foreach (var role in roles)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }
        }

        return true;
    }
}