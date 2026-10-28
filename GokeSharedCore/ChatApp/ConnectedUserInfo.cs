namespace GokeSharedCore.ChatApp
{
    public sealed class ConnectedUserInfo
    {
        public string UserId { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string ConnectionId { get; set; } = string.Empty;

        public bool IsAuthenticated { get; set; }
    }
}
