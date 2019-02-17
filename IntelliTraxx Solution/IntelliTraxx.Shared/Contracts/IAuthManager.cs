using System;
using System.Collections.Generic;

namespace IntelliTraxx.Shared
{
    public interface IAuthManager
    {
        Guid LogonUser(string username, string password);
        List<string> GetUserRoles(Guid userId);
    }
}
