﻿using Microsoft.IdentityModel.Tokens;
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
    /// <summary>
    /// Add this attribute to a controller or action to enforce token-based authentication
    /// </summary>
    public class TokenRequired : Attribute, IAuthenticationFilter
    {
        #region Static members for configurations and initialization of reusable components

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

        #endregion

        /// <summary>
        /// Only one instance of this attribute can be applied to a single class or function
        /// </summary>
        public bool AllowMultiple
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Deny access if a token is missing from the header Authorization, or invalid; otherwise, let the request goes through.
        /// </summary>
        public Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            Trace.Assert(TokenValidator != null, "TokenValidator is required for authentication");
            Trace.Assert(TokenValidations != null, "TokenValidations are required for authentication");


            AuthenticationHeaderValue authentication = null;
            SecurityToken securityToken = null;
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

        /// <summary>
        /// Return a challenge response with the realm included in the header WWW-Authenticate
        /// </summary>
        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            context.Result = new ChallengeResult(context.Result, Realm);
            return Task.FromResult(0);
        }

        /// <summary>
        /// The challenge response to unauthenticated requests
        /// </summary>
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