namespace GokeSharedCore.ChatApp
{
    public sealed class GroupMessageRequest
    {
        public required string GroupId { get; set; }
        public required string Content { get; set; }
    }


}
