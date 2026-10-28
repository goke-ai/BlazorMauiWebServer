using System;
using System.Collections.Generic;
using System.Text;

namespace GokeSharedCore.ChatApp
{

    public interface IChatRepository
    {
        Task<Chat?> GetPrivateChatAsync(string user1, string user2);
        Task<Chat> CreatePrivateChatAsync(string user1, string user2);
        Task<List<Message>> GetMessagesAsync(string user1, string user2, int page, int pageSize);
        Task<List<ChatSummaryDto>> GetChatsAsync(string userId);
    }
}
