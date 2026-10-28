namespace GokeSharedCore.ChatApp
{
    public class UserPresenceDto
    {
        public string UserId { get; set; } = string.Empty;
        public bool IsOnline { get; set; }
        public bool IsTyping { get; set; }
        public DateTime? LastSeenUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }

        internal static UserPresenceDto? FromEntity(UserPresence? presence)
        {
            if (presence == null) return null;
            return new UserPresenceDto
            {
                UserId = presence.UserId,
                IsOnline = presence.IsOnline,
                IsTyping = presence.IsTyping,
                LastSeenUtc = presence.LastSeenUtc,
                UpdatedAtUtc = presence.UpdatedAtUtc
            };
        }
    }

}
