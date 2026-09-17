using System.Security.Claims;

namespace GokeApp.Services
{
    public class AuthenticationState(ClaimsPrincipal user)
    {
        private ClaimsPrincipal user = user;

        public ClaimsPrincipal User => user;
    }
}