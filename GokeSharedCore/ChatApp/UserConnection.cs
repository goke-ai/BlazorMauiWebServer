namespace GokeSharedCore.ChatApp
{
    public sealed class UserConnection
    {
        public string ConnectionId { get; set; } = string.Empty;

        public string UserId { get; set; } = string.Empty;

        public string DeviceId { get; set; } = string.Empty;

        public string Platform { get; set; } = string.Empty;

        public DateTime ConnectedAtUtc { get; set; }

        public DateTime LastActivityUtc { get; set; }
    }
}
