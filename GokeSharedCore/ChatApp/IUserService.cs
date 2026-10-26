using Goke.Core.Chat;

namespace GokeSharedCore.ChatApp
{
    public interface IUserService
    {
        Task<List<User>> GetUsersAsync();
        Task<List<User>> GetOnlineUsersAsync();
        Task<User?> GetUserByIdAsync(string userId);

    }
}