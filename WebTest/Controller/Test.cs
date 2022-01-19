using Microsoft.AspNetCore.Mvc;

namespace WebTest.Controller
{
    [ApiController]
    [Route("api/{controller}")]
    public class Test : ControllerBase
    {
        private readonly ILogger logger;

        public Test(ILogger logger)
        {
            this.logger = logger;
        }
        [HttpGet]
        public string Hello()
        {
            logger.LogInformation("hello");
            return $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        }
    }
}
