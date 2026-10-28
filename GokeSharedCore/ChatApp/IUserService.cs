namespace GokeSharedCore.ChatApp
{
    public interface IUserService
    {
        Task<List<UiChatUser>> GetUsersAsync();
        Task<List<UiChatUser>> GetOnlineUsersAsync();
        Task<UiChatUser?> GetUserByIdAsync(string userId);

    }
}