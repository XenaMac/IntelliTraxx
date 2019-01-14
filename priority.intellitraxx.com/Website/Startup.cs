using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Base_AVL.Startup))]
namespace Base_AVL
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
