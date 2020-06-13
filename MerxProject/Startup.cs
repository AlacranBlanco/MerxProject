using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MerxProject.Startup))]
namespace MerxProject
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
