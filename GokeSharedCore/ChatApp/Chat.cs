namespace GokeSharedCore.ChatApp
{
    public sealed class Chat
    {
        public Guid Id { get; set; }
        public bool IsGroup { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<ChatParticipant> Participants { get; set; } = new List<ChatParticipant>();
    }


}
