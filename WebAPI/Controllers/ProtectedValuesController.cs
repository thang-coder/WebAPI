using System.Collections.Generic;
using System.Web.Http;
using WebAPI.Attributes;

namespace WebAPI.Controllers
{
    [TokenRequired]
    public class ProtectedValuesController : ApiController
    {
        // GET api/ProtectedValues
        public IEnumerable<string> Get()
        {
            return new string[] { "silver", "gold", "diamond" };
        }
    }
}