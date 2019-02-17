namespace IntelliTraxx.Shared
{
    public interface IUserSecurity
    {
        UserSecurityContext GetCurrentUserSecurityContext();
    }
}
