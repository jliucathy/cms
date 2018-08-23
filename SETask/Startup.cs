using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SETask.Startup))]
namespace SETask
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
