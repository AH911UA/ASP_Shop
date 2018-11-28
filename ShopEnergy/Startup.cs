using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ShopEnergy.Startup))]
namespace ShopEnergy
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
