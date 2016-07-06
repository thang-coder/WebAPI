using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace WebAPI.ActionFilters
{
    public class AdminOnly : AuthorizeAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            var principal = actionContext.RequestContext.Principal as ClaimsPrincipal;
            var adminClaim = principal?.FindFirst("admin")?.Value;

            if (principal != null && string.Compare(adminClaim, bool.TrueString, ignoreCase: true) == 0)
            {
                base.OnAuthorization(actionContext);
                return;
            }

            actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
        }
    }
}