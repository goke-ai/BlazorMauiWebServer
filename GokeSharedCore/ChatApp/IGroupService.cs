using GokeSharedCore.ChatApp;

namespace GokeWebServer.Hubs
{
    public interface IGroupService
    {
        Task DeleteAsync(string groupId);
        Task<List<Group>> GetUserGroupsAsync(string userId);
        Task RenameAsync(string groupId, string name);
    }

    public sealed class GroupService : IGroupService
    {
        private readonly ChatDbContext db;
        public GroupService(ChatDbContext db)
        {
            this.db = db;
        }
        public async Task DeleteAsync(string groupId)
        {
            var group = await Task.FromResult(db.Groups.Find(f => f.Id == Guid.Parse(groupId)));
            if (group != null)
            {
                db.Groups.Remove(group);
                await db.SaveChangesAsync();
            }
        }
        public async Task<List<Group>> GetUserGroupsAsync(string userId)
        {
            return await Task.FromResult(db.GroupMembers
                .Where(x => x.UserId == userId)
                .Select(x => x.Group!)
                .ToList());
        }
        public async Task RenameAsync(string groupId, string name)
        {
            var group = await Task.FromResult(db.Groups.Find(f => f.Id == Guid.Parse(groupId)));
            if (group != null)
            {
                group.Name = name;
                await db.SaveChangesAsync();
            }
        }
    }

}