namespace GokeSharedCore.ChatApp
{
    // blocked user
    public sealed class BlockedUser
    {
        public string UserId { get; set; } = string.Empty;
        public string BlockedUserId { get; set; } = string.Empty;
        public DateTime BlockedAtUtc { get; set; }
    }
}
