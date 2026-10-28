namespace GokeSharedCore.ChatApp
{
    public sealed class SendMessageRequest
    {
        public required string ReceiverId { get; set; }
        public required string Content { get; set; }
        public MessageType MessageType { get; set; } = MessageType.Text;
        public string? ReplyToMessageId { get; set; }
    }

}
