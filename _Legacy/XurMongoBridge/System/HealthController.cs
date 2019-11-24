using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace XurMongoBridge.Controllers
{
    [Route("system/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        // GET system/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return Ok();
        }
    }
}