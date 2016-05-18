using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using DataConvertor.App_Start;
using Newtonsoft.Json.Serialization;

namespace DataConvertor
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            
             GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.ContractResolver =
                new CamelCasePropertyNamesContractResolver();
             GlobalConfiguration.Configuration.Formatters.JsonFormatter.UseDataContractJsonSerializer = false;

            // Add a reference here to the new MediaTypeFormatter that adds text/plain support
            // GlobalConfiguration.Configuration.Formatters.Insert(0, new TextMediaTypeFormatter());
        }
    }
}
