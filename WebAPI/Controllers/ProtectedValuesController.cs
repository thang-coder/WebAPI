using System.Collections.Generic;
using System.Web.Http;

namespace WebAPI.Controllers
{
    [Authorize]
    public class ProtectedValuesController : ApiController
    {
        // GET api/ProtectedValues
        public IEnumerable<string> Get()
        {
            return new string[] { "silver", "gold", "diamond" };
        }
    }
}