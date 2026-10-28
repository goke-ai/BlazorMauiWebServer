namespace GokeSharedCore.ChatApp
{
    public class ChatDbContext
    {
        public List<Message> Messages { get; set; } = new List<Message>();
        public List<Chat> Chats { get; set; } = new List<Chat>();
        public List<ChatParticipant> ChatParticipants { get; set; } = new List<ChatParticipant>();
        public List<GroupMember> GroupMembers { get; set; } = new List<GroupMember>();
        public List<Group> Groups { get; set; } = new List<Group>();

        public Task SaveChangesAsync()
        {
            // In a real implementation, this would save changes to the database.
            return Task.CompletedTask;
        }
    }
}
