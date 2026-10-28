using System.Collections.Concurrent;

namespace GokeSharedCore.ChatApp
{
    public interface IUserPresenceService
    {
        Task SetOnlineAsync(string userId);

        Task SetOfflineAsync(string userId);

        Task<UserPresenceDto?> GetAsync(string userId);

        // getonlineusers
        Task<List<string>> GetOnlineUsersAsync();
    }

    public class UserPresenceService(IUserConnectionStore connectionStore) : IUserPresenceService
    {
        private readonly ConcurrentDictionary<string, UserPresence> _userPresences = new();

        public async Task SetOnlineAsync(string userId)
        {
            var isOnline = await connectionStore.IsOnlineAsync(userId);
            if (isOnline)
            {
                var presence = new UserPresence
                {
                    UserId = userId,
                    IsOnline = true,
                    LastSeenUtc = null,
                    IsTyping = false,
                    UpdatedAtUtc = DateTime.UtcNow
                };
                _userPresences[userId] = presence;
            }
        }
        public async Task SetOfflineAsync(string userId)
        {
            var isOnline = await connectionStore.IsOnlineAsync(userId);
            if (!isOnline)
            {
                var presence = new UserPresence
                {
                    UserId = userId,
                    IsOnline = false,
                    LastSeenUtc = DateTime.UtcNow,
                    IsTyping = false,
                    UpdatedAtUtc = DateTime.UtcNow
                };
                _userPresences[userId] = presence;
            }
        }
        public Task<UserPresenceDto?> GetAsync(string userId)
        {
            _userPresences.TryGetValue(userId, out var presence);
            return Task.FromResult(UserPresenceDto.FromEntity(presence));
        }

        public Task<List<string>> GetOnlineUsersAsync()
        {
            return Task.FromResult(_userPresences.Values.Where(p => p.IsOnline).Select(p => p.UserId).ToList());
        }
    }
}
