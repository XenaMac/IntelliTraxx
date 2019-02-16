using System;

namespace IntelliTraxx.Shared.Identity
{
    public class UserSecurityContext
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }        
    }
}
