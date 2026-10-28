namespace GokeSharedCore.ChatApp
{
    public sealed class Message
    {
        public Guid Id { get; set; }

        public Guid ChatId { get; set; }

        public Guid SenderId { get; set; }

        public Guid? ReceiverId { get; set; }

        public string Content { get; set; } = string.Empty;

        public MessageType Type { get; set; }

        public MessageStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? DeliveredAt { get; set; }

        public DateTime? ReadAt { get; set; }
    }






}
