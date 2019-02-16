using IntelliTraxx.Shared.Identity;

namespace IntelliTraxx.Shared.Contracts
{
    public interface IUserSecurity
    {
        UserSecurityContext GetCurrentUserSecurityContext();
    }
}
