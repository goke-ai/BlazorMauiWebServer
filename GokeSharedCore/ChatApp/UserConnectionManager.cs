namespace GokeSharedCore.ChatApp
{
    public sealed class UserConnectionManager : IUserConnectionManager
    {
        private readonly Dictionary<string, HashSet<string>> _connections = new Dictionary<string, HashSet<string>>();
        private readonly Dictionary<string, HashSet<string>> _groupUsers = new Dictionary<string, HashSet<string>>();

        public Task AddAsync(string userId, string connectionId)
        {
            lock (_connections)
            {
                if (!_connections.TryGetValue(userId, out HashSet<string>? value))
                {
                    value = new HashSet<string>();
                    _connections[userId] = value;
                }

                value.Add(connectionId);
            }
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string userId, string connectionId)
        {
            lock (_connections)
            {
                if (_connections.TryGetValue(userId, out HashSet<string>? value))
                {
                    value.Remove(connectionId);
                    if (value.Count == 0)
                    {
                        _connections.Remove(userId);
                    }
                }
            }
            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<string>> GetConnectionsAsync(string userId)
        {
            lock (_connections)
            {
                if (_connections.TryGetValue(userId, out HashSet<string>? value))
                {
                    return Task.FromResult((IReadOnlyCollection<string>)value.ToList());
                }
            }
            return Task.FromResult((IReadOnlyCollection<string>)Array.Empty<string>());
        }


        public bool IsConnected(string userId)
        {
            lock (_connections)
            {
                return _connections.TryGetValue(userId, out HashSet<string>? value) && value.Count > 0;
            }
        }

        public Task<bool> IsOnlineAsync(string userId)
        {
            lock (_connections)
            {
                return Task.FromResult(_connections.TryGetValue(userId, out HashSet<string>? value) && value.Count > 0);
            }
        }

        public Task AddToGroupAsync(string userId, string groupId)
        {
            lock (_groupUsers)
            {
                if (_groupUsers.TryGetValue(groupId, out HashSet<string>? users))
                {
                    users.Add(userId);
                }
                else
                {
                    users = new HashSet<string> { userId };
                    _groupUsers[groupId] = users;
                }
            }
            return Task.CompletedTask;
        }

        public Task<List<string>> GetConnectionsInGroupAsync(string groupId)
        {
            lock (_groupUsers)
            {
                if (_groupUsers.TryGetValue(groupId, out HashSet<string>? users))
                {
                    var connections = users.Select(userId => _connections[userId]).SelectMany(x => x).ToList();
                    return Task.FromResult(connections);
                }
            }
            return Task.FromResult(new List<string>());
        }

        public Task RemoveGroupAsync(string groupId)
        {
            lock (_groupUsers)
            {
                if (_groupUsers.TryGetValue(groupId, out HashSet<string>? value))
                {
                    value.Clear();
                    _groupUsers.Remove(groupId);
                }
            }
            return Task.CompletedTask;
        }

    }
}
