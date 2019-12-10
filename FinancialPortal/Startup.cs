using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(FinancialPortal.Startup))]
namespace FinancialPortal
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
