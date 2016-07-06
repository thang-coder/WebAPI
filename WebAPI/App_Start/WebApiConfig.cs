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
            // Configure the hashing function, then use it
            HmacSignatureFactory.Secret = "eyJzdWIiOiJkY3RoYW5nQGdtYWlsLmNvbSIsIm5hbWUiOiJUaGFuZyBEdW9uZyIsImFkbWluIjp0cnVlfQ";
            TokenRequired.Use(typeof(HMACSHA256));

            // Configure the realm for challenge responses
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