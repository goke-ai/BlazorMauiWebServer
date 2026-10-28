namespace GokeSharedCore.ChatApp
{
    public sealed class MessageDto
    {
        public Guid Id { get; set; }

        public required string SenderId { get; set; }

        public string? ReceiverId { get; set; }

        public required string Content { get; set; }

        public MessageStatus Status { get; set; }

        public MessageType Type { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? ReadAt { get; set; }

        public static MessageDto FromEntity(Message message)
        {
            return new MessageDto
            {
                Id = message.Id,
                SenderId = message.SenderId.ToString(),
                ReceiverId = message.ReceiverId?.ToString(),
                Content = message.Content,
                Status = message.Status,
                Type = message.Type,
                CreatedAt = message.CreatedAt,
                ReadAt = message.ReadAt
            };
        }
    }


}
