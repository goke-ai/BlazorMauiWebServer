using System.Collections.Concurrent;

namespace GokeSharedCore.ChatApp
{

    public class UserPresenceService(IUserConnectionManager connectionManager) : IUserPresenceService
    {
        private readonly ConcurrentDictionary<string, UserPresence> _userPresences = new();

        public async Task SetOnlineAsync(string userId)
        {
            var isOnline = await connectionManager.IsOnlineAsync(userId);
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
            var isOnline = await connectionManager.IsOnlineAsync(userId);
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
        public Task<UserPresence?> GetAsync(string userId)
        {
            _userPresences.TryGetValue(userId, out var presence);
            return Task.FromResult(presence);
        }

        public Task<List<string>> GetOnlineUsersAsync()
        {
            return Task.FromResult(_userPresences.Values.Where(p => p.IsOnline).Select(p => p.UserId).ToList());
        }
    }
}
