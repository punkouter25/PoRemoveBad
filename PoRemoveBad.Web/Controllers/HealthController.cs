using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace PoRemoveBad.Web.Controllers
{
    /// <summary>
    /// Controller that provides health check endpoints for the application.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ILogger<HealthController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="HealthController"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        public HealthController(ILogger<HealthController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Gets the health status of the API.
        /// </summary>
        /// <returns>A 200 OK response if the API is healthy.</returns>
        [HttpGet]
        public IActionResult Get()
        {
            _logger.LogInformation("Health check requested at {Time}", DateTime.UtcNow);
            return Ok(new { status = "Healthy", timestamp = DateTime.UtcNow });
        }
    }
}