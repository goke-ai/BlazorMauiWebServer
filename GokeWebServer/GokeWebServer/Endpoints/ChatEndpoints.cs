using GokeSharedCore.ChatApp;

namespace GokeWebServer.Endpoints
{
    public static class ChatEndpoints
    {
        public static IEndpointRouteBuilder MapChatEndpoints(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("api/chat/users", async (IUserService userService) =>
            {
                var users = await userService.GetUsersAsync();

                return Results.Ok(users);
            }).RequireAuthorization();

            endpoints.MapGet("api/chat/users/{userId}", async (string userId, IUserService userService) =>
            {
                var user = await userService.GetUserByIdAsync(userId);
                if (user == null)
                    return Results.NotFound();

                return Results.Ok(user);
            }).RequireAuthorization();

            endpoints.MapGet("api/chat/online-users", async (IUserService userService) =>
            {
                var users = await userService.GetOnlineUsersAsync();
                return Results.Ok(users);
            }).RequireAuthorization();

            return endpoints;
        }
    }

}