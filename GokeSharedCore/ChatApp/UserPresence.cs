namespace GokeSharedCore.ChatApp
{
    public sealed class UserPresence
    {
        public string UserId { get; set; } = string.Empty;

        public bool IsOnline { get; set; }


        public bool IsTyping { get; set; }

        public DateTime? LastSeenUtc { get; set; }

        public DateTime UpdatedAtUtc { get; set; }
    }
}
