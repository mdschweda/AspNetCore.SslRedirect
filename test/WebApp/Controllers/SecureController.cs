using Microsoft.AspNetCore.Mvc;
using MS.AspNetCore.Ssl.Mvc;

namespace WebApp.Controllers {

    [RequireSsl]
    [Route("api/[controller]")]
    public class SecureController : Controller {

        [HttpGet("{id}")]
        public string Get(int id) => $"value{id}";

    }
}
