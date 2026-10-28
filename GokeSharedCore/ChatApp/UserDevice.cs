namespace GokeSharedCore.ChatApp
{
    // user device
    public sealed class UserDevice
    {
        public string UserId { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty;
        public DateTime LastActiveAtUtc { get; set; }
    }
}
