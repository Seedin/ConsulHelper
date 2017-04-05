using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BitAuto.Ucar.Utils.Common;

namespace HttpDemo.Controllers
{
    [Route("api/[controller]")]
    [Route("/")]
    public class ValuesController : Controller
    {
        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "F:Config:httpdemo:test",
                ConsulHelper.Instance.GetKeyValue("F:Config:httpdemo:test") };

        }

        [HttpHead]
        public void Head()
        {
            ConsulHelper.Instance.GetHashCode();
        }



        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(string id)
        {
            return ConsulHelper.Instance.GetKeyValue("F:Config:httpdemo:"+ id);
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
            ConsulHelper.Instance.SetKeyValue("F:Config:httpdemo:test", value);
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
            ConsulHelper.Instance.SetKeyValue("F:Config:httpdemo:test", value);
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            ConsulHelper.Instance.SetKeyValue("F:Config:httpdemo:test", string.Empty);
        }
    }
}
