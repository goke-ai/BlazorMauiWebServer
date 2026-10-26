namespace Goke.Core.Chat
{
    public sealed class User
    {
        public string UserId { get; set; }    

        public string FullName { get; set; }

        public string DisplayName{ get; set; } 

        public string? PhotoUrl { get; set; }

        public string? Bio { get; set; }

        public bool IsOnline { get; set; }

        public DateTime LastSeenUtc { get; set; }

        public DateTime CreatedAtUtc { get; set; }
    }

    public enum MessageStatus
    {
        Pending = 0,
        Sent = 1,
        Delivered = 2,
        Read = 3
    }

    public enum MessageType
    {
        Text = 0,
        Image = 1,
        Video = 2,
        File = 3,
        Audio = 4,
        Location = 5,
        Contact = 6,
        Sticker = 7,
        System = 8

    }

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

    public sealed class ChatSummaryDto
    {
        public Guid ChatId { get; set; }
        public string? LastMessageContent { get; set; }
        public DateTime? LastMessageTimestamp { get; set; }
    }

    public sealed class SendMessageRequest
    {
        public required string ReceiverId { get; set; }
        public required string Content { get; set; }
        public MessageType MessageType { get; set; } = MessageType.Text;
    }

    public sealed class GroupMessageRequest
    {
        public required string GroupId { get; set; }
        public required string Content { get; set; }
    }

    public sealed class Chat
    {
        public Guid Id { get; set; }
        public bool IsGroup { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<ChatParticipant> Participants { get; set; } = new List<ChatParticipant>();
    }

    public sealed class ChatParticipant
    {
        public Guid ChatId { get; set; }
        public Guid UserId { get; set; }
    }


}
