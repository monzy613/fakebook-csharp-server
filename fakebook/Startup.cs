using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(fakebook.Startup))]
namespace fakebook
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
