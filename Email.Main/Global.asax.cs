using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Email.Main
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        private static Timer _keepAliveTimer;
        private static readonly HttpClient _httpClient = new HttpClient();
        private static Timer _timer;

        protected void Application_Start()
        {
            double interval = 5 * 60 * 1000; // Runs every 5 minutes

            _timer = new Timer(new TimerCallback(ExecuteEmailJob), null, 0, (int)interval);

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        private static void ExecuteEmailJob(object state)
        {
            try
            {
                var controller = new Email.Main.Controllers.EmailController();
                controller.working();
            }
            catch (Exception)
            {
            }
        }
    }
}
