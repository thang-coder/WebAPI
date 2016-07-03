using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Filters;
using System.Web.Http.Results;

namespace WebAPI.Attributes
{
    public class TokenRequired : Attribute, IAuthenticationFilter
    {
        public static Uri Realm { get; internal set; }

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
            var authHeader = context.Request.Headers.Authorization;
            if (authHeader == null || authHeader.Scheme != "Bearer")
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