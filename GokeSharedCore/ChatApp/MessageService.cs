using Goke.Core.Chat;

namespace GokeSharedCore.ChatApp
{
    public sealed class MessageService(ChatDbContext db) : IMessageService
    {
        public async Task<MessageDto> CreateMessageAsync(
            string senderId,
            string receiverId,
            string content,
            MessageType type)
        {
            var chat =
                await GetOrCreatePrivateChat(
                    senderId,
                    receiverId);

            var message = new Message
            {
                Id = Guid.NewGuid(),
                ChatId = chat.Id,
                SenderId = Guid.Parse(senderId),
                ReceiverId = Guid.Parse(receiverId),
                Content = content,
                Type = type,
                Status = MessageStatus.Sent,
                CreatedAt = DateTime.UtcNow
            };

            db.Messages.Add(message);

            await db.SaveChangesAsync();

            return MessageDto.FromEntity(message);

        }

        public async Task<MessageDto> CreateGroupMessageAsync(
            string senderId,
            string groupId,
            string content)
        {
            var message = new Message
            {
                Id = Guid.NewGuid(),
                ChatId = Guid.Parse(groupId),
                SenderId = Guid.Parse(senderId),
                Content = content,
                Type = MessageType.Text,
                Status = MessageStatus.Sent,
                CreatedAt = DateTime.UtcNow
            };

            db.Messages.Add(message);

            await db.SaveChangesAsync();

            return MessageDto.FromEntity(message);
        }

        public async Task<MessageDto> MessageDeliveredAsync(
            Guid messageId)
        {
            var message =
                 db.Messages
                    .FirstOrDefault(
                        x => x.Id == messageId);

            if (message is null)
                throw new Exception("Message not found");

            if (message.Status <
                MessageStatus.Delivered)
            {
                message.Status =
                    MessageStatus.Delivered;

                message.DeliveredAt =
                    DateTime.UtcNow;

                await db.SaveChangesAsync();
            }

            return MessageDto.FromEntity(message);
        }

        public async Task<MessageDto>
            MessageReadAsync(
                Guid messageId,
                string readerId)
        {
            var message =
                db.Messages
                    .FirstOrDefault(
                        x => x.Id == messageId);

            if (message == null)
                throw new Exception("Message not found");

            if (message.Status !=
                MessageStatus.Read)
            {
                message.Status =
                    MessageStatus.Read;

                message.ReadAt =
                    DateTime.UtcNow;

                await db.SaveChangesAsync();
            }

            return MessageDto.FromEntity(message);
        }

        public async Task<IReadOnlyList<MessageDto>>
            GetConversationAsync(
                string user1,
                string user2,
                int page,
                int pageSize)
        {
            var messages =
                db.Messages
                .Where(x =>
                    (x.SenderId ==
                        Guid.Parse(user1)
                     &&
                     x.ReceiverId ==
                        Guid.Parse(user2))
                     ||
                    (x.SenderId ==
                         Guid.Parse(user2)
                     &&
                     x.ReceiverId ==
                         Guid.Parse(user1)))
                .OrderByDescending(
                    x => x.CreatedAt)
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToList();

            return messages
                .Select(MessageDto.FromEntity)
                .ToList();
        }

        public async Task<IReadOnlyList<MessageDto>>
           GetConversationAsync(
               string user1)
        {
            var messages =
                db.Messages
                .Where(x => x.SenderId == Guid.Parse(user1)
                     || x.ReceiverId == Guid.Parse(user1))
                .OrderByDescending(
                    x => x.CreatedAt)
                //.Skip(page * pageSize)
                //.Take(pageSize)
                .ToList();

            return messages
                .Select(MessageDto.FromEntity)
                .ToList();
        }

        public async Task<IReadOnlyList<ChatSummaryDto>>
            GetChatsAsync(string userId)
        {
            var chats =
                db.ChatParticipants
                .Where(x =>
                    x.UserId ==
                    Guid.Parse(userId))
                .Select(x =>
                    new ChatSummaryDto
                    {
                        ChatId = x.ChatId
                    })
                .ToList();

            return chats;
        }

        private async Task<Chat>
            GetOrCreatePrivateChat(
                string user1,
                string user2)
        {
            var chat =
                db.Chats
                //.Include(x => x.Participants)
                .FirstOrDefault(x => !x.IsGroup &&
                    x.Participants.Any(p => p.UserId == Guid.Parse(user1))
                    &&
                    x.Participants.Any(p => p.UserId == Guid.Parse(user2)));

            if (chat != null)
                return chat;

            chat = new Chat
            {
                Id = Guid.NewGuid(),
                IsGroup = false,
                CreatedAt = DateTime.UtcNow
            };

            db.Chats.Add(chat);

            db.ChatParticipants.Add(
                new ChatParticipant
                {
                    ChatId = chat.Id,
                    UserId = Guid.Parse(user1)
                });

            db.ChatParticipants.Add(
                new ChatParticipant
                {
                    ChatId = chat.Id,
                    UserId = Guid.Parse(user2)
                });

            await db.SaveChangesAsync();

            return chat;
        }
    }
}
