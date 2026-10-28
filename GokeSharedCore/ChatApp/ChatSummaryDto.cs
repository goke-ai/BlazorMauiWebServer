namespace GokeSharedCore.ChatApp
{
    public sealed class ChatSummaryDto
    {
        public Guid ChatId { get; set; }
        public string? LastMessageContent { get; set; }
        public DateTime? LastMessageTimestamp { get; set; }
    }


}
