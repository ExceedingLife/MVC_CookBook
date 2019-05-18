using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MVC_CookBook.Startup))]
namespace MVC_CookBook
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
