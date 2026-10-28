namespace GokeSharedCore.ChatApp
{

    public class Group
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public List<GroupMember> Members { get; set; } = new List<GroupMember>();
    }

    public sealed class GroupMember
    {
        public Guid GroupId { get; set; }

        public string UserId { get; set; } = string.Empty;

        public GroupRole Role { get; set; }

        public DateTime JoinedAtUtc { get; set; }

        public Group? Group { get; set; } 
    }

}
