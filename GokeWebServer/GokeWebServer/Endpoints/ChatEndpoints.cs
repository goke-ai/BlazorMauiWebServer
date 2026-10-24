using Goke.Core.Chat;
using GokeWebServer.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GokeWebServer.Endpoints
{
    public static class ChatEndpoints
    {
        public static IEndpointRouteBuilder MapChatEndpoints(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("api/chat/users", async (UserManager<ApplicationUser> userManager) =>
            {
                var users = await userManager.Users
                    .Select(u => new User
                    {
                        UserId = u.Id,
                        FullName = u.UserName,
                        DisplayName = u.UserName,
                        Bio = u.Id + " " + u.UserName,
                        PhotoUrl = null,
                        IsOnline = false
                    })
                    .ToListAsync();

                return Results.Ok(users);
            }).RequireAuthorization();

            return endpoints;
        }
    }
}