using Microsoft.AspNetCore.Mvc;
using MS.AspNetCore.Ssl.Mvc;

namespace WebApp.Controllers {

    [Route("api/[controller]")]
    public class TestController : Controller {

        [HttpGet("{id}")]
        public string Get(int id) => $"value{id}";

        [RequireSsl]
        [HttpGet("Secure/{id}")]
        public string GetSecure(int id) => $"secureValue{id}";

        [HttpPost]
        public string Post([FromBody]string value) => value?.ToUpper();

        [RequireSsl]
        [HttpPost("Secure")]
        public string SecurePost([FromBody]string value) => $"secure{value?.ToUpper()}";

    }
}
