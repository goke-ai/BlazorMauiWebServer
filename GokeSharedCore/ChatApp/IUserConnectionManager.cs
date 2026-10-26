namespace GokeSharedCore.ChatApp
{
    public interface IUserConnectionManager
    {
        Task AddAsync(string userId, string connectionId);
        Task RemoveAsync(string userId, string connectionId);
        bool IsConnected(string userId);
        Task AddToGroupAsync(string userId, string groupId);
        Task<List<string>> GetConnectionsInGroupAsync(string groupId);
        Task RemoveGroupAsync(string groupId);
    }
}
