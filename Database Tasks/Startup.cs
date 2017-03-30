using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Database_Login_Logger.Models;
using Hangfire;
using Hangfire.Dashboard;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Database_Login_Logger.Startup))]

namespace Database_Login_Logger
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var listener =
                (HttpListener)app.Properties["System.Net.HttpListener"];

            listener.AuthenticationSchemes =
                AuthenticationSchemes.IntegratedWindowsAuthentication;

            app.UseHangfireDashboard("", new DashboardOptions
            {
                Authorization = new List<IDashboardAuthorizationFilter>
            {
                new HangfireAuthorizationFilter()
            }
            });

            app.UseHangfireServer();
        }
    }

    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var owinContext = new OwinContext(context.GetOwinEnvironment());

            if (owinContext.Authentication.User != null && owinContext.Authentication.User.IsInRole("ARI IT DEVELOPMENT GLOBAL"))
            {
                return true;
            }

            return false;

        }
    }

}
