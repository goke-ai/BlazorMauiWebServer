namespace GokeSharedCore.ChatApp
{


    public sealed class ChatUser
    {
        public required string UserId { get; set; }

        public string Username { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string? FullName { get; set; }

        public string? PhotoUrl { get; set; }

        public string? Bio { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAtUtc { get; set; }

        public DateTime UpdatedAtUtc { get; set; }
    }
}
