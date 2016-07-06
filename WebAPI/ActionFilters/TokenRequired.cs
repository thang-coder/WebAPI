using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Filters;
using System.Web.Http.Results;
using WebAPI.SignatureTypes;

namespace WebAPI.ActionFilters
{
    public class TokenRequired : Attribute, IAuthenticationFilter
    {
        static TokenRequired()
        {
            TokenValidator = new JwtSecurityTokenHandler();
            Trace.Assert(Realm == null, "The realm should be set by the WebApiConfig class.");
            Trace.Assert(TokenValidations == null, "The token validation parameters should be initialized by the Use() function.");
        }

        public static Uri Realm { get; internal set; }

        public static TokenValidationParameters TokenValidations { get; internal set; }

        // NuGet: Install-Package System.IdentityModel.Tokens.Jwt
        public static ISecurityTokenValidator TokenValidator { get; internal set; }

        internal static void Use(Type hashType)
        {
            var map = new Dictionary<Type, TokenValidationParameters>
            {
                [typeof(HMACSHA256)] = HmacSignatureFactory.ValidationParameters
            };

            TokenValidationParameters validations = null;
            if (!map.TryGetValue(hashType, out validations))
            {
                throw new NotSupportedException($"Hashing algorithm of type '{hashType}' is not supported.");
            }

            TokenValidations = validations;
        }

        // Only one instance of this attribute can be applied to a single class or function.
        public bool AllowMultiple
        {
            get
            {
                return false;
            }
        }

        public Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            AuthenticationHeaderValue authentication = null;
            SecurityToken securityToken = null;

            Trace.Assert(TokenValidator != null, "TokenValidator is required for authentication");
            Trace.Assert(TokenValidations != null, "TokenValidations are required for authentication");

            try
            {
                authentication = context.Request.Headers.Authorization;
                context.Principal = TokenValidator.ValidateToken(authentication.Parameter, TokenValidations, out securityToken);
            }
            catch (Exception error)
            {
                Trace.TraceError($"Missing or invalid token. Error: {error}");
            }

            if (authentication == null || authentication.Scheme != "Bearer" || securityToken == null)
            {
                context.ErrorResult = new UnauthorizedResult(new AuthenticationHeaderValue[0], context.Request);
            }

            return Task.FromResult(0);
        }

        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            context.Result = new ChallengeResult(context.Result, Realm);
            return Task.FromResult(0);
        }

        private class ChallengeResult : IHttpActionResult
        {
            private IHttpActionResult contextResult;
            private Uri realm;

            public ChallengeResult(IHttpActionResult contextResult, Uri realm)
            {
                this.contextResult = contextResult;
                this.realm = realm;
            }

            public async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
            {
                var response = await contextResult.ExecuteAsync(cancellationToken);
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                    response.Headers.WwwAuthenticate.Add(new AuthenticationHeaderValue("Bearer", $"realm=\"{realm}\""));
                return response;
            }
        }
    }
}