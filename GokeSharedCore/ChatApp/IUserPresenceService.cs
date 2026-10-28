namespace GokeSharedCore.ChatApp
{
    public interface IUserPresenceService
    {
        Task SetOnlineAsync(string userId);

        Task SetOfflineAsync(string userId);

        Task<UserPresence?> GetAsync(string userId);

        // getonlineusers
        Task<List<string>> GetOnlineUsersAsync();
    }
}
