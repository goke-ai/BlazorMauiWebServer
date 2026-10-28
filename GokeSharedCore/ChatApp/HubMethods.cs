using System;
using System.Collections.Generic;
using System.Text;

namespace GokeSharedCore.ChatApp
{
    public static class HubMethods
    {
        public static class On
        {
            public const string ConnectionEstablished = nameof(ConnectionEstablished);

            public const string UserConnected = nameof(UserConnected);

            public const string UserDisconnected = nameof(UserDisconnected);

            public const string ReceivePrivateMessage = nameof(ReceivePrivateMessage);

            public const string MessageSent = nameof(MessageSent);

            public const string MessageRead = nameof(MessageRead);

            public const string MessageDelivered = nameof(MessageDelivered);

            public const string StartTyping = nameof(StartTyping);

            public const string StopTyping = nameof(StopTyping);

            public const string GroupJoined = nameof(GroupJoined);

            public const string GroupLeft = nameof(GroupLeft);

            public const string ReceiveGroupMessage = nameof(ReceiveGroupMessage);
            public const string ReceiveOffer = nameof(ReceiveOffer);
            public const string ReceiveAnswer = nameof(ReceiveAnswer);
            public const string ReceiveIceCandidate = nameof(ReceiveIceCandidate);
            public const string GroupRemoved = nameof(GroupRemoved);
            public const string GroupDeleted = nameof(GroupDeleted);
            public const string GroupRenamed = nameof(GroupRenamed);
            public const string PresenceUpdate = nameof(PresenceUpdate);
        }

        public static class Send
        {
            public const string SendPrivateMessage = nameof(SendPrivateMessage);
            public const string StartTyping = nameof(StartTyping);
            public const string StopTyping = nameof(StopTyping);
            public const string MessageRead = nameof(MessageRead);
            public const string MessageDelivered = nameof(MessageDelivered);
            public const string JoinGroup = nameof(JoinGroup);
            public const string LeaveGroup = nameof(LeaveGroup);
            public const string SendGroupMessage = nameof(SendGroupMessage);
            public const string SendOffer = nameof(SendOffer);
            public const string SendAnswer = nameof(SendAnswer);
            public const string SendIceCandidate = nameof(SendIceCandidate);
            public const string RemoveGroup = nameof(RemoveGroup);

        }


        public static class Invoke
        {
            internal static string ShareLocation = nameof(ShareLocation);
            public const string GetPresence = nameof(GetPresence);
            public const string GetOnlineUsers = nameof(GetOnlineUsers);
        }
    }
}
