using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RestApi.Test
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthTestController : ControllerBase
    {
        [Authorize]
        [HttpGet("test-auth")]
        public IActionResult Test()
        {
            return Ok("Auth works");
        }
    }
}
