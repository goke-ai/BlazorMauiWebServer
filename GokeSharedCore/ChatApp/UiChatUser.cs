namespace GokeSharedCore.ChatApp
{
    public sealed class UiChatUser
    {
        public string UserId { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string? PhotoUrl { get; set; }

        public bool IsOnline { get; set; }

        public DateTime? LastSeenUtc { get; set; }

        public string? LastMessage { get; set; }

        public DateTime? LastMessageUtc { get; set; }

        public int UnreadCount { get; set; }

        public bool IsTyping { get; set; }
    }
}
