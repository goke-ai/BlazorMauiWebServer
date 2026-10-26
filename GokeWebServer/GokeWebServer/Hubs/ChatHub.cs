using Goke.Core.Chat;
using GokeSharedCore.ChatApp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

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

            await Clients.Others.SendAsync("UserConnected", userId);

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


}
