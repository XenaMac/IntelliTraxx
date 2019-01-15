using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(IntelliTraxx.Startup))]
namespace IntelliTraxx
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
