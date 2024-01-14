/// Purpose: Defines the EchoController class.
/// 

using Microsoft.AspNetCore.Mvc;

namespace EchoServer.Echo;

[ApiController]
[Route("[controller]")]
public class EchoController : ControllerBase
{
    private readonly ILogger<EchoController> _logger;
    private readonly IEchoService _echoService;

    public EchoController(ILogger<EchoController> logger, IEchoService echoService)
    {
        _logger = logger;
        _echoService = echoService;
    }

    [HttpPost]
    [HttpPut]
    [HttpPatch]
    [HttpGet]
    [HttpDelete]
    public async Task<IActionResult> Echo()
    {
        using StreamReader reader = new(Request.Body, leaveOpen: false);
        var bodyAsString = await reader.ReadToEndAsync();

        EchoResponse echoRc = await _echoService.Get(Request, HttpContext, bodyAsString);
        return Ok(echoRc);
    }
}
