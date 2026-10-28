using GokeSharedCore.ChatApp;
using GokeWebServer.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GokeWebServer.Services
{
    public class ChatUserService(UserManager<ApplicationUser> userManager) : IUserService
    {
        public async Task<List<UiChatUser>> GetUsersAsync()
        {
            var users = await userManager.Users
                   .Select(u => new UiChatUser
                   {
                       UserId = u.Id,
                       //FullName = u.UserName,
                       DisplayName = u.UserName,
                       PhotoUrl = null,
                       IsOnline = false,
                       LastSeenUtc = null
                   })
                   .ToListAsync();

            return users;
        }


        public async Task<UiChatUser?> GetUserByIdAsync(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return null;
            return new UiChatUser
            {
                UserId = user.Id,
                //FullName = user.UserName,
                DisplayName = user.UserName,
                //Bio = user.Id + " " + user.UserName,
                PhotoUrl = null,
                IsOnline = false,
                LastSeenUtc = null
            };
        }

        //online users
        public async Task<List<UiChatUser>> GetOnlineUsersAsync()
        {
            var users = await userManager.Users
                   .Select(u => new UiChatUser
                   {
                       UserId = u.Id,
                       //FullName = u.UserName,
                       DisplayName = u.UserName,
                       //Bio = u.Id + " " + u.UserName,
                       PhotoUrl = null,
                       IsOnline = true,
                       LastSeenUtc = null
                   })
                   .ToListAsync();
            return users;
        }
    }

}