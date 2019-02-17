using IntelliTraxx.Shared;

namespace IntelliTraxx.WebApi.Helpers
{
    public interface IAppBaseContracts
    {
        IUserSecurity GetUserSecurity();
    }

    public class AppBaseContracts : IAppBaseContracts
    {
        public AppBaseContracts(IUserSecurity userSecurity)
        {
            UserSecurity = userSecurity;
        }

        private IUserSecurity UserSecurity { get; }

        public IUserSecurity GetUserSecurity()
        {
            return UserSecurity;
        }
    }
}