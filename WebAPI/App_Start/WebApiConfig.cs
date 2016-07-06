using System;
using System.Security.Cryptography;
using System.Web.Http;
using WebAPI.Attributes;
using WebAPI.SignatureTypes;

namespace WebAPI
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Authentication configurations
            HmacSignatureFactory.Secret = "THIS IS A BIG SECRET"; // TODO: load secret string from config file

            // there is no specific order for Use and Realm
            TokenRequired.Use(typeof(HMACSHA256));
            TokenRequired.Realm = new Uri("http://localhost:6301");

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}