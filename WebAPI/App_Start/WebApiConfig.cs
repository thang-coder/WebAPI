using System;
using System.Security.Cryptography;
using System.Web.Http;
using WebAPI.ActionFilters;
using WebAPI.SignatureTypes;

namespace WebAPI
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Configure the hashing function, then use it
            HmacSignatureFactory.Secret = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiYWRtaW4iOnRydWV9TJVA95OrM7E2cBab30RMHrHDcEfxjoYZgeFONFh7HgQ";
            TokenRequired.Use(typeof(HMACSHA256));

            // Configure the realm for challenge responses
            TokenRequired.Realm = new Uri("http://JSONWebToken.azurewebsites.net");

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