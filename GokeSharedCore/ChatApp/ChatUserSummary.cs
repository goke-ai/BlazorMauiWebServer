namespace GokeSharedCore.ChatApp
{
    public sealed class ChatUserSummary
    {
        public string UserId { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string? PhotoUrl { get; set; }
    }
}
