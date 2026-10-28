namespace GokeSharedCore.ChatApp
{
    public interface IChatService
    {
        Task<Chat> GetOrCreatePrivateChatAsync(string user1, string user2);
        Task<List<Message>> GetConversationAsync(string user1, string user2, int page, int pageSize);
        Task<List<ChatSummaryDto>> GetChatsAsync(string userId);
    }
}
