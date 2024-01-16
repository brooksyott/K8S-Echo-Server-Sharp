using Microsoft.AspNetCore.Mvc;
using Serilog.Core;

[ApiController]
[Route("/diagnostics")]
public class DiagnosticsController : ControllerBase
{
    private readonly LoggingLevelSwitch _levelSwitch;
    private readonly ILogger<DiagnosticsController> _logger;

    public DiagnosticsController(ILogger<DiagnosticsController> logger, LoggingLevelSwitch levelSwitch)
    {
        _logger = logger;
        _levelSwitch = levelSwitch;
    }

    [HttpPost("loglevel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult SwitchLoggingLevel([FromBody] LogLevelSettings logLevelSettings)
    {
        _logger.LogWarning("Switching logging level to {level}", logLevelSettings.level);
        if (Enum.TryParse(logLevelSettings.level, out Serilog.Events.LogEventLevel newLoggingLevel))
        {
            _levelSwitch.MinimumLevel = newLoggingLevel;
            _logger.LogCritical("Test CRITICAL log");
            _logger.LogError("Test ERROR log");
            _logger.LogWarning("Test WARNING log");
            _logger.LogInformation("Test INFORMATION log");
            _logger.LogDebug("Test DEBUG log");
            _logger.LogTrace("Test TRACE log");
            return Ok();
        }

        return BadRequest();
    }

    [HttpGet("loglevel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult CurrentLoggingLevel()
    {
        return Ok(new LogLevelSettings { level = _levelSwitch.MinimumLevel.ToString() });
    }
}