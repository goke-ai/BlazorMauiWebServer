using Goke.Core.Chat;
using GokeSharedCore.ChatApp;
using GokeWebServer.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GokeWebServer.Services
{
    public class UserService(UserManager<ApplicationUser> userManager) : IUserService
    {
        public async Task<List<User>> GetUsersAsync()
        {
            var users = await userManager.Users
                   .Select(u => new User
                   {
                       UserId = u.Id,
                       FullName = u.UserName,
                       DisplayName = u.UserName,
                       Bio = u.Id + " " + u.UserName,
                       PhotoUrl = null,
                       IsOnline = false
                   })
                   .ToListAsync();

            return users;
        }


        public async Task<User?> GetUserByIdAsync(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return null;
            return new User
            {
                UserId = user.Id,
                FullName = user.UserName,
                DisplayName = user.UserName,
                Bio = user.Id + " " + user.UserName,
                PhotoUrl = null,
                IsOnline = false
            };
        }

        //online users
        public async Task<List<User>> GetOnlineUsersAsync()
        {
            var users = await userManager.Users
                   .Select(u => new User
                   {
                       UserId = u.Id,
                       FullName = u.UserName,
                       DisplayName = u.UserName,
                       Bio = u.Id + " " + u.UserName,
                       PhotoUrl = null,
                       IsOnline = true
                   })
                   .ToListAsync();
            return users;
        }
    }

}