using Goke.Core.Chat;

namespace GokeSharedCore.ChatApp
{
    public interface IMessageRepository
    {
        Task<Message> CreateMessageAsync(Message message);
        Task<Message> CreateGroupMessageAsync(Message message);
        Task<Message?> GetMessageByIdAsync(Guid messageId);
        Task UpdateMessageAsync(Message message);
    }
}
