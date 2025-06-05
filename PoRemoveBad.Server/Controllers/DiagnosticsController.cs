using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace PoRemoveBad.Web.Controllers
{
    /// <summary>
    /// Controller for handling diagnostics operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DiagnosticsController : ControllerBase
    {
        private readonly ILogger<DiagnosticsController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagnosticsController"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        public DiagnosticsController(ILogger<DiagnosticsController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Writes diagnostic logs to the log.txt file.
        /// </summary>
        /// <param name="logs">The collection of log entries.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        [HttpPost("log")]
        public async Task<IActionResult> LogDiagnostics([FromBody] List<LogEntry> logs)
        {
            try
            {
                _logger.LogInformation("Received {Count} diagnostic log entries", logs.Count);

                // Create a new log.txt file (not appending)
                string logFilePath = Path.Combine(Directory.GetCurrentDirectory(), "log.txt");
                using (StreamWriter writer = new StreamWriter(logFilePath, false))
                {
                    await writer.WriteLineAsync($"PoRemoveBad Diagnostic Log - {DateTime.Now}");
                    await writer.WriteLineAsync("================================================");
                    
                    foreach (var log in logs)
                    {
                        await writer.WriteLineAsync($"[{log.Timestamp:yyyy-MM-dd HH:mm:ss}] [{log.Level}] {log.Message}");
                    }
                    
                    await writer.WriteLineAsync("================================================");
                    await writer.WriteLineAsync($"Log generated at: {DateTime.Now}");
                }

                _logger.LogInformation("Diagnostic logs written to log.txt");
                return Ok(new { success = true, message = "Logs written to log.txt" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error writing diagnostic logs to file");
                return StatusCode(500, new { success = false, message = $"Error writing logs: {ex.Message}" });
            }
        }

        /// <summary>
        /// Log entry data transfer object.
        /// </summary>
        public class LogEntry
        {
            /// <summary>
            /// Gets or sets the timestamp of the log entry.
            /// </summary>
            public DateTime Timestamp { get; set; }
            
            /// <summary>
            /// Gets or sets the log level.
            /// </summary>
            public string Level { get; set; } = "INFO";
            
            /// <summary>
            /// Gets or sets the log message.
            /// </summary>
            public string Message { get; set; } = "";
        }
    }
}