namespace GokeSharedCore.ChatApp
{
    public interface IUserConnectionStore
    {
        Task AddAsync(UserConnection connection);
        Task<IReadOnlyCollection<UserConnection>> GetConnectionsAsync(string userId);
        bool IsConnected(string userId);
        Task<bool> IsOnlineAsync(string userId);
        Task RemoveAsync(string connectionId);
    }
}