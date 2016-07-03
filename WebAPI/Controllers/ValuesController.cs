using System.Collections.Generic;
using System.Web.Http;

namespace WebAPI.Controllers
{
    // Authorization not required
    public class ValuesController : ApiController
    {
        // GET api/Values
        public IEnumerable<string> Get()
        {
            return new string[] { "air", "water" };
        }
    }
}