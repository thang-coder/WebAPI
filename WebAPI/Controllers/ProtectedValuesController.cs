using System.Collections.Generic;
using System.Web.Http;
using WebAPI.ActionFilters;

namespace WebAPI.Controllers
{
    [TokenRequired]
    public class ProtectedValuesController : ApiController
    {
        private static List<string> protectedValues;

        static ProtectedValuesController()
        {
            protectedValues = new List<string> { "silver", "gold", "diamond" };
        }

        // GET api/ProtectedValues
        public IEnumerable<string> Get()
        {
            return protectedValues;
        }

        [AdminOnly]
        // POST api/ProtectedValues
        public string Post(string value)
        {
            protectedValues.Add(value);
            return value;
        }
    }
}