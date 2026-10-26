using Goke.Aspnet.Core;
using Goke.Core.Chat;
using Goke.Core.Options;
using Microsoft.Extensions.Options;

namespace GokeSharedCore.ChatApp
{
    public class ChatHubClientService(IHubClientFactory factory) : IAsyncDisposable
    {
        private HubClientService? hub;


        public bool IsConnected => hub?.IsConnected ?? false;

        public async Task InitializeAsync()
        {
            hub ??= await factory.CreateHubAsync();

            hub.Reconnected += async () =>
            {
                if (Reconnected is not null)
                {
                    await Reconnected.Invoke();
                }
            };
        }

        public event Func<Task>? Reconnected;

        public IDisposable OnUserConnected(
            Action<string> handler)
        {
            return hub!.On(
                "UserConnected",
                handler);
        }

        public IDisposable OnUserDisconnected(
            Action<string> handler)
        {
            return hub!.On(
                "UserDisconnected",
                handler);
        }

        public IDisposable OnReceivePrivateMessage(
            Action<MessageDto> handler)
        {
            return hub!.On(
                "ReceivePrivateMessage",
                handler);
        }

        //MessageSent
        public IDisposable OnMessageSent(
            Action<MessageDto> handler)
        {
            return hub!.On(
                "MessageSent",
                handler);
        }

        public IDisposable OnReceiveGroupMessage(
            Action<MessageDto> handler)
        {
            return hub!.On(
                "ReceiveGroupMessage",
                handler);
        }

        public IDisposable OnStartTyping(
            Action<string> handler)
        {
            return hub!.On(
                "StartTyping",
                handler);
        }

        public IDisposable OnStopTyping(
            Action<string> handler)
        {
            return hub!.On(
                "StopTyping",
                handler);
        }

        //MessageRead
        public IDisposable OnMessageRead(
            Action<string> handler)
        {
            return hub!.On(
                "MessageRead",
                handler);
        }

        public IDisposable OnMessageDelivered(
            Action<string> handler)
        {
            return hub!.On(
                "MessageDelivered",
                handler);
        }

        public IDisposable OnMessageSeen(
            Action<string> handler)
        {
            return hub!.On(
                "MessageSeen",
                handler);
        }

        public IDisposable OnMessageDeleted(
            Action<string> handler)
        {
            return hub!.On(
                "MessageDeleted",
                handler);
        }

        public IDisposable OnGroupJoined(
            Action<string> handler)
        {
            return hub!.On(
                "GroupJoined",
                handler);
        }

        public IDisposable OnGroupLeft(
            Action<string> handler)
        {
            return hub!.On(
                "GroupLeft",
                handler);
        }

        public IDisposable OnGroupRemoved(
            Action<string> handler)
        {
            return hub!.On(
                "GroupRemoved",
                handler);
        }

        public Task SendPrivateMessageAsync(
            SendMessageRequest request)
        {
            return hub!.SendAsync(
                "SendPrivateMessage",
                request);
        }

        public Task<bool> ShareLocationAsync(
            SendMessageRequest request)
        {
            return hub!.InvokeAsync<bool>(
                "ShareLocation",
                request);
        }

        public Task<List<User>> GetConnectedUsersAsync(
            SendMessageRequest request)
        {
            return hub!.InvokeAsync<List<User>>(
                "GetConnectedUsers",
                request);
        }

        public async ValueTask DisposeAsync()
        {
            if (hub is not null)
            {
                await hub!.DisposeAsync();
                hub= null;
            }
        }

    }
}
