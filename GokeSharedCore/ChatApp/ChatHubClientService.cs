using Goke.Aspnet.Core;
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
                HubMethods.On.UserConnected,
                handler);
        }

        public IDisposable OnUserDisconnected(
            Action<string> handler)
        {
            return hub!.On(
                HubMethods.On.UserDisconnected,
                handler);
        }

        public IDisposable OnConnectionEstablished(
                    Action<string> handler)
        {
            return hub!.On(
                HubMethods.On.ConnectionEstablished,
                handler);
        }

        public IDisposable OnReceivePrivateMessage(
            Action<MessageDto> handler)
        {
            return hub!.On(
                HubMethods.On.ReceivePrivateMessage,
                handler);
        }

        //MessageSent
        public IDisposable OnMessageSent(
            Action<MessageDto> handler)
        {
            return hub!.On(
                HubMethods.On.MessageSent,
                handler);
        }

        public IDisposable OnReceiveGroupMessage(
            Action<MessageDto> handler)
        {
            return hub!.On(
                HubMethods.On.ReceiveGroupMessage,
                handler);
        }

        public IDisposable OnStartTyping(
            Action<string> handler)
        {
            return hub!.On(
                HubMethods.On.StartTyping,
                handler);
        }

        public IDisposable OnStopTyping(
            Action<string> handler)
        {
            return hub!.On(
                HubMethods.On.StopTyping,
                handler);
        }

        //MessageRead
        public IDisposable OnMessageRead(
            Action<string> handler)
        {
            return hub!.On(
                HubMethods.On.MessageRead,
                handler);
        }

        public IDisposable OnMessageDelivered(
            Action<string> handler)
        {
            return hub!.On(
                HubMethods.On.MessageDelivered,
                handler);
        }

        public IDisposable OnGroupJoined(
            Action<string> handler)
        {
            return hub!.On(
                HubMethods.On.GroupJoined,
                handler);
        }

        public IDisposable OnGroupLeft(
            Action<string> handler)
        {
            return hub!.On(
                HubMethods.On.GroupLeft,
                handler);
        }

        public IDisposable OnGroupRemoved(
            Action<string> handler)
        {
            return hub!.On(
                HubMethods.On.GroupRemoved,
                handler);
        }

        //presenceupdate
        public IDisposable OnPresenceUpdate(
            Action<UserPresence> handler)
        {
            return hub!.On(
                HubMethods.On.PresenceUpdate,
                handler);
        }


        // send methods
        public Task SendPrivateMessageAsync(
            SendMessageRequest request)
        {
            return hub!.SendAsync(
                HubMethods.Send.SendPrivateMessage,
                request);
        }


        // invoke methods
        public Task<bool> ShareLocationAsync(
            SendMessageRequest request)
        {
            return hub!.InvokeAsync<bool>(
                HubMethods.Invoke.ShareLocation,
                request);
        }

        public Task<UserPresence?> GetPresenceAsync(string userId)
        {
            return hub!.InvokeAsync<UserPresence?>(
                HubMethods.Invoke.GetPresence,
                userId);
        }

        public Task<List<string>> GetOnlineUsersAsync()
        {
            return hub!.InvokeAsync<List<string>>(
                HubMethods.Invoke.GetOnlineUsers);
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
