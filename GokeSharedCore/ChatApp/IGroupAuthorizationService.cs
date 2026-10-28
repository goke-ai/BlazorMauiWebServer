namespace GokeSharedCore.ChatApp
{
    public interface IGroupAuthorizationService
    {
        Task<bool> CanAccessGroupAsync(
            string userId,
            string groupId);

        Task<bool> IsAdminAsync(
            string userId,
            string groupId);

        Task<bool> IsOwnerAsync(
            string userId,
            string groupId);
    }

    public sealed class GroupAuthorizationService
    : IGroupAuthorizationService
    {
        private readonly ChatDbContext db;

        public GroupAuthorizationService(
            ChatDbContext db)
        {
            this.db = db;
        }

        public async Task<bool> CanAccessGroupAsync(
            string userId,
            string groupId)
        {
            return db.GroupMembers
                .Any(x =>
                    x.GroupId.ToString() == groupId &&
                    x.UserId == userId);
        }

        public async Task<bool> IsAdminAsync(
            string userId,
            string groupId)
        {
            return db.GroupMembers
                .Any(x =>
                    x.GroupId.ToString() == groupId &&
                    x.UserId == userId &&
                    (x.Role == GroupRole.Admin ||
                     x.Role == GroupRole.Owner));
        }

        public async Task<bool> IsOwnerAsync(
            string userId,
            string groupId)
        {
            return db.GroupMembers
                .Any(x =>
                    x.GroupId.ToString() == groupId &&
                    x.UserId == userId &&
                    x.Role == GroupRole.Owner);
        }
    }

}
