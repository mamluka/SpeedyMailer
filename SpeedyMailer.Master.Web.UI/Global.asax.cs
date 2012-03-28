﻿using System.Web.Mvc;
using System.Web.Routing;
using Bootstrap;
using Bootstrap.AutoMapper;

namespace SpeedyMailer.Master.Web.UI
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
               "Deals", // Route name
               "Deals/{JsonObject}", // URL with parameters
               new { controller = "Deals", action = "RedirectToDeal", JsonObject = "{}" } // Parameter defaults
           );

            routes.MapRoute(
               "Unsubscribe", // Route name
               "Unsubscribe/{JsonObject}", // URL with parameters
               new { controller = "Contacts", action = "Unsubscribe", JsonObject = "{}" } // Parameter defaults
           );

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

            

        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            Bootstrapper.With.AutoMapper().Start();

         
        }
    }
}