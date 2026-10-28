namespace GokeSharedCore.ChatApp
{
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
        Task<IReadOnlyList<MessageDto>> GetConversationAsync(string user1);
    }
}
