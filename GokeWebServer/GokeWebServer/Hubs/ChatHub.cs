using Azure.Core;
using Goke.Core.Chat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace GokeWebServer.Hubs
{
    [Authorize]
    public sealed class ChatHub(
        IUserConnectionManager connectionManager,
        IMessageService messageService) : Hub
    {

        #region Connection

        public override async Task OnConnectedAsync()
        {
            string userId = GetUserId();

            await connectionManager.AddAsync(userId, Context.ConnectionId);

            await Clients.All.SendAsync("UserConnected", userId);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            string userId = GetUserId();

            await connectionManager.RemoveAsync(userId, Context.ConnectionId);

            bool isStillConnected = connectionManager.IsConnected(userId);

            if (!isStillConnected)
            {
                await Clients.Others.SendAsync("UserDisconnected", userId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        #endregion

        #region Private Message

        public async Task SendPrivateMessage(SendMessageRequest request)
        {
            string senderId = GetUserId();

            var message = await messageService.CreateMessageAsync(
                                senderId,
                                request.ReceiverId,
                                request.Content,
                                request.MessageType);

            await Clients.User(request.ReceiverId)
                .SendAsync("ReceivePrivateMessage", message);

            await Clients.Caller.SendAsync("MessageSent", message);
        }

        #endregion

        #region Typing Indicator

        public async Task StartTyping(string receiverId)
        {
            await Clients.User(receiverId)
                .SendAsync("UserTyping", GetUserId());
        }

        public async Task StopTyping(string receiverId)
        {
            await Clients.User(receiverId)
                .SendAsync("UserStoppedTyping", GetUserId());
        }

        #endregion

        #region Read Receipt

        public async Task MessageRead(Guid messageId)
        {
            var message = await messageService.MessageReadAsync(messageId, GetUserId());

            await Clients.User(message.SenderId.ToString())
                .SendAsync("MessageRead", messageId);
        }

        #endregion

        #region Delivery Receipt

        public async Task MessageDelivered(Guid messageId)
        {
            var message = await messageService.MessageDeliveredAsync(messageId);

            await Clients.User(message.SenderId.ToString())
                .SendAsync("MessageDelivered", messageId);
        }

        #endregion

        #region Groups

        public async Task JoinGroup(string groupId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupId);

            // save the groupId in the user's connection state if needed
            string userId = GetUserId();
            await connectionManager.AddToGroupAsync(userId, groupId);

            await Clients.User(userId).SendAsync("GroupJoined", groupId);
        }

        public async Task LeaveGroup(string groupId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId);
            
            await Clients.User(GetUserId()).SendAsync("GroupLeft", groupId);
        }        

        public async Task SendGroupMessage(GroupMessageRequest request)
        {
            string senderId = GetUserId();

            var message = await messageService.CreateGroupMessageAsync(
                                                senderId,
                                                request.GroupId,
                                                request.Content);

            await Clients.Group(request.GroupId)
                .SendAsync("ReceiveGroupMessage", message);
        }

        #endregion

        #region Calls

        public async Task SendOffer(string receiverId, string sdpOffer)
        {
            await Clients.User(receiverId)
                .SendAsync("ReceiveOffer", GetUserId(), sdpOffer);
        }

        public async Task SendAnswer(string receiverId, string sdpAnswer)
        {
            await Clients.User(receiverId)
                .SendAsync("ReceiveAnswer", GetUserId(), sdpAnswer);
        }

        public async Task SendIceCandidate(string receiverId, string candidate)
        {
            await Clients.User(receiverId)
                .SendAsync("ReceiveIceCandidate", GetUserId(), candidate);
        }

        #endregion


        // remove a group
        public async Task RemoveGroup(string groupId)
        {
            // Notify all clients in the group that it has been removed
            await Clients.Group(groupId).SendAsync("GroupRemoved", groupId);

            // Remove all connections from the group
            var connections = await connectionManager.GetConnectionsInGroupAsync(groupId);
            foreach (var connectionId in connections)
            {
                await Groups.RemoveFromGroupAsync(connectionId, groupId);
            }

            // Remove the group from the connection manager
            await connectionManager.RemoveGroupAsync(groupId);
        }

        private string GetUserId()
        {
            var userId = Context.UserIdentifier ?? throw new InvalidOperationException("UserIdentifier is null");
            return userId;
        }
    }       

    public interface IChatRepository
    {
        Task<Chat?> GetPrivateChatAsync(string user1, string user2);
        Task<Chat> CreatePrivateChatAsync(string user1, string user2);
        Task<List<Message>> GetMessagesAsync(string user1, string user2, int page, int pageSize);
        Task<List<ChatSummaryDto>> GetChatsAsync(string userId);
    }

    public interface IMessageRepository
    {
        Task<Message> CreateMessageAsync(Message message);
        Task<Message> CreateGroupMessageAsync(Message message);
        Task<Message?> GetMessageByIdAsync(Guid messageId);
        Task UpdateMessageAsync(Message message);
    }

    public interface IChatService
    {
        Task<Chat> GetOrCreatePrivateChatAsync(string user1, string user2);
        Task<List<Message>> GetConversationAsync(string user1, string user2, int page, int pageSize);
        Task<List<ChatSummaryDto>> GetChatsAsync(string userId);
    }

    public interface IUserConnectionManager
    {
        Task AddAsync(string userId, string connectionId);
        Task RemoveAsync(string userId, string connectionId);
        bool IsConnected(string userId);
        Task AddToGroupAsync(string userId, string groupId);
        Task<List<string>> GetConnectionsInGroupAsync(string groupId);
        Task RemoveGroupAsync(string groupId);
    }

    public sealed class UserConnectionManager : IUserConnectionManager
    {
        private readonly Dictionary<string, HashSet<string>> _connections = new Dictionary<string, HashSet<string>>();
        private readonly Dictionary<string, HashSet<string>> _groupUsers = new Dictionary<string, HashSet<string>>();

        public Task AddAsync(string userId, string connectionId)
        {
            lock (_connections)
            {
                if (!_connections.TryGetValue(userId, out HashSet<string>? value))
                {
                    value = new HashSet<string>();
                    _connections[userId] = value;
                }

                value.Add(connectionId);
            }
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string userId, string connectionId)
        {
            lock (_connections)
            {
                if (_connections.TryGetValue(userId, out HashSet<string>? value))
                {
                    value.Remove(connectionId);
                    if (value.Count == 0)
                    {
                        _connections.Remove(userId);
                    }
                }
            }
            return Task.CompletedTask;
        }

        public bool IsConnected(string userId)
        {
            lock (_connections)
            {
                return _connections.TryGetValue(userId, out HashSet<string>? value) && value.Count > 0;
            }
        }

        public Task AddToGroupAsync(string userId, string groupId)
        {
            lock (_groupUsers)
            {
                if (_groupUsers.TryGetValue(groupId, out HashSet<string>? users))
                {
                    users.Add(userId);
                }
                else
                {
                    users = new HashSet<string> { userId };
                    _groupUsers[groupId] = users;
                }
            }
            return Task.CompletedTask;
        }

        public Task<List<string>> GetConnectionsInGroupAsync(string groupId)
        {
            lock (_groupUsers)
            {
                if (_groupUsers.TryGetValue(groupId, out HashSet<string>? users))
                {
                    var connections = users.Select(userId => _connections[userId]).SelectMany(x => x).ToList();
                    return Task.FromResult(connections);
                }
            }
            return Task.FromResult(new List<string>());
        }

        public Task RemoveGroupAsync(string groupId)
        {
            lock (_groupUsers)
            {
                if (_groupUsers.TryGetValue(groupId, out HashSet<string>? value))
                {
                    value.Clear();
                    _groupUsers.Remove(groupId);
                }
            }
            return Task.CompletedTask;
        }
    }

    public interface IMessageService
    {
        Task<MessageDto> CreateMessageAsync(
            string senderId,
            string receiverId,
            string content,
            MessageType type);

        Task<MessageDto> CreateGroupMessageAsync(
            string senderId,
            string groupId,
            string content);

        Task<MessageDto> MessageDeliveredAsync(
            Guid messageId);

        Task<MessageDto> MessageReadAsync(
            Guid messageId,
            string readerId);

        Task<IReadOnlyList<MessageDto>>
            GetConversationAsync(
                string user1,
                string user2,
                int page,
                int pageSize);

        Task<IReadOnlyList<ChatSummaryDto>>
            GetChatsAsync(string userId);
    }

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

    public class ChatDbContext
    {
        public List<Message> Messages { get; set; } = new List<Message>();
        public List<Chat> Chats { get; set; } = new List<Chat>();
        public List<ChatParticipant> ChatParticipants { get; set; } = new List<ChatParticipant>();
        public Task SaveChangesAsync()
        {
            // In a real implementation, this would save changes to the database.
            return Task.CompletedTask;
        }
    }

}
