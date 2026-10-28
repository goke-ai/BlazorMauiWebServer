using GokeSharedCore.ChatApp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace GokeWebServer.Hubs
{
    [Authorize]
    public sealed class ChatHub(
        IUserConnectionManager connectionManager,
        IUserPresenceService userPresenceService,
        IMessageService messageService,
        IGroupAuthorizationService groupAuth,
        IGroupService groupService
        ) : Hub
    {

        private string UserId
        {
            get
            {
                var userId = Context.UserIdentifier ?? throw new InvalidOperationException("UserIdentifier is null");
                return userId;
            }
        }

        #region Connection

        public override async Task OnConnectedAsync()
        {
            var groups = await groupService.GetUserGroupsAsync(UserId);

            foreach (var group in groups)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, group.Id.ToString());
            }

            await connectionManager.AddAsync(UserId, Context.ConnectionId);

            var connections = await connectionManager.GetConnectionsAsync(UserId);

            var isFirstConnection = connections.Count == 1;

            if (isFirstConnection)
            {
                await userPresenceService.SetOnlineAsync(UserId);

                await Clients.Others.SendAsync(HubMethods.On.UserConnected, UserId);
            }

            await Clients.Caller.SendAsync(HubMethods.On.ConnectionEstablished, Context.ConnectionId);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await connectionManager.RemoveAsync(UserId, Context.ConnectionId);

            bool isStillConnected = connectionManager.IsConnected(UserId);

            if (!isStillConnected)
            {
                await Clients.Others.SendAsync(HubMethods.On.UserDisconnected, UserId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        #endregion

        #region Presence

        public async Task<UserPresence?> GetPresence(string userId)
        {
            return await userPresenceService.GetAsync(userId);
        }

        public async Task<List<string>> GetOnlineUsers()
        {
            return await userPresenceService.GetOnlineUsersAsync();
        }

        //PresenceUpdate event is sent to all clients when a user's presence changes
        public async Task UpdatePresence(UserPresence presence)
        {
            await Clients.Others.SendAsync(HubMethods.On.PresenceUpdate, presence);
        }

        #endregion

        #region Private Message

        public async Task SendPrivateMessage(SendMessageRequest request)
        {
            string senderId = UserId;

            var message = await messageService.CreateMessageAsync(
                                senderId,
                                request.ReceiverId,
                                request.Content,
                                request.MessageType);

            await Clients.User(request.ReceiverId)
                .SendAsync(HubMethods.On.ReceivePrivateMessage, message);

            await Clients.Caller.SendAsync(HubMethods.On.MessageSent, message);
        }

        #endregion

        #region Typing Indicator

        public async Task StartTyping(string receiverId)
        {
            await Clients.User(receiverId)
                .SendAsync(HubMethods.On.StartTyping, UserId);
        }

        public async Task StopTyping(string receiverId)
        {
            await Clients.User(receiverId)
                .SendAsync(HubMethods.On.StopTyping, UserId);
        }

        #endregion

        #region Read Receipt

        public async Task MessageRead(Guid messageId)
        {
            var message = await messageService.MessageReadAsync(messageId, UserId);

            await Clients.User(message.SenderId.ToString())
                .SendAsync(HubMethods.On.MessageRead, messageId, DateTime.UtcNow);
        }

        #endregion

        #region Delivery Receipt

        public async Task MessageDelivered(Guid messageId)
        {
            var message = await messageService.MessageDeliveredAsync(messageId);

            await Clients.User(message.SenderId.ToString())
                .SendAsync(HubMethods.On.MessageDelivered, messageId, DateTime.UtcNow);
        }

        #endregion

        #region Groups

        public async Task JoinGroup(string groupId)
        {
            if (!await groupAuth.CanAccessGroupAsync(UserId, groupId))
            {
                throw new HubException(
                    "You are not a member of this group.");
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, groupId);

            await Clients.Group(groupId)
                .SendAsync(
                    HubMethods.On.GroupJoined,
                    groupId,
                    UserId);
        }

        public async Task LeaveGroup(string groupId)
        {
            if (!await groupAuth.CanAccessGroupAsync(UserId, groupId))
            {
                return;
            }
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId);
            
            await Clients.User(UserId).SendAsync(HubMethods.On.GroupLeft, groupId);
        }        

        public async Task SendGroupMessage(GroupMessageRequest request)
        {
            if (!await groupAuth.CanAccessGroupAsync(UserId, request.GroupId))
            {
                throw new HubException("Unauthorized group access.");
            }

            string senderId = UserId;

            var message = await messageService.CreateGroupMessageAsync(
                                                senderId,
                                                request.GroupId,
                                                request.Content);

            await Clients.Group(request.GroupId)
                .SendAsync(HubMethods.On.ReceiveGroupMessage, message);
        }

        public async Task RenameGroup(string groupId, string name)
        {
            if (!await groupAuth.IsAdminAsync(
                UserId,
                groupId))
            {
                throw new HubException(
                    "Admin permission required.");
            }

            await groupService.RenameAsync(groupId, name);

            await Clients.Group(groupId).
                SendAsync(HubMethods.On.GroupRenamed, groupId, name);
        }


        public async Task DeleteGroup(string groupId)
        {
            if (!await groupAuth.IsOwnerAsync(
                UserId,
                groupId))
            {
                throw new HubException(
                    "Only the owner can delete a group.");
            }

            await groupService.DeleteAsync(groupId);

            await Clients.Group(groupId)
                .SendAsync(
                    HubMethods.On.GroupDeleted,
                    groupId);
        }

        #endregion

        #region Calls

        public async Task SendOffer(string receiverId, string sdpOffer)
        {
            await Clients.User(receiverId)
                .SendAsync(HubMethods.On.ReceiveOffer, UserId, sdpOffer);
        }

        public async Task SendAnswer(string receiverId, string sdpAnswer)
        {
            await Clients.User(receiverId)
                .SendAsync(HubMethods.On.ReceiveAnswer, UserId, sdpAnswer);
        }

        public async Task SendIceCandidate(string receiverId, string candidate)
        {
            await Clients.User(receiverId)
                .SendAsync(HubMethods.On.ReceiveIceCandidate, UserId, candidate);
        }

        #endregion


        #region Location

        public async Task<bool> ShareLocation(SendMessageRequest request)
        {
            string senderId = UserId;
            var message = await messageService.CreateMessageAsync(
                                senderId,
                                request.ReceiverId,
                                request.Content,
                                request.MessageType);
            await Clients.User(request.ReceiverId)
                .SendAsync(HubMethods.On.ReceivePrivateMessage, message);
            await Clients.Caller.SendAsync(HubMethods.On.MessageSent, message);
            
            return true;
        }

        #endregion

    }

}
