using System.Collections.Concurrent;

namespace GokeSharedCore.ChatApp
{
    public class UserConnectionStore : IUserConnectionStore
    {
        private readonly ConcurrentDictionary<string,
            ConcurrentDictionary<string, UserConnection>>
            _connections = new();

        public Task AddAsync(UserConnection connection)
        {
            var userConnections = _connections.GetOrAdd(connection.UserId, _ => new ConcurrentDictionary<string, UserConnection>());
            userConnections[connection.ConnectionId] = connection;
            return Task.CompletedTask;

        }

        public Task<IReadOnlyCollection<UserConnection>> GetConnectionsAsync(string userId)
        {
            return Task.FromResult<IReadOnlyCollection<UserConnection>>(
                _connections.TryGetValue(userId, out var userConnections)
                    ? userConnections.Values.ToList()
                    : new List<UserConnection>());
        }

        public bool IsConnected(string userId)
        {
            return _connections.TryGetValue(userId, out var userConnections) && userConnections.Count > 0;
        }

        public Task<bool> IsOnlineAsync(string userId)
        {
            return Task.FromResult(_connections.ContainsKey(userId));
        }

        public Task RemoveAsync(string connectionId)
        {
            foreach (var userConnections in _connections.Values)
            {
                if (userConnections.TryRemove(connectionId, out _))
                {
                    return Task.CompletedTask;
                }
            }
            return Task.CompletedTask;
        }
    }


}
