using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc;
using EchoServer.Kubernetes;

namespace EchoServer.Probe;

[ApiController]
[Route("/health")]
public class ProbeController : ControllerBase
{
    private readonly ILogger<ProbeController> _logger;
    private readonly IProbeService _probeService;

    private readonly KubernetesHelper _k8sHelper = new KubernetesHelper();

    public ProbeController(ILogger<ProbeController> logger, IProbeService probeService)
    {
        _logger = logger;
        _probeService = probeService;
    }

    [HttpGet("live")]
    public async Task<IActionResult> LivenessProbe()
    {
        var (response, httpCode) = await _probeService.IsAlive();

        return StatusCode(httpCode, response);
    }

    [HttpPost("live")]
    public async Task<IActionResult> SetLivenessProbe([FromBody] ProbeConfiguration probeConfiguration)
    {
        _probeService.SetIsAlive(probeConfiguration);
        var (response, httpCode) = await _probeService.IsAlive();

        return StatusCode(httpCode, response);
    }


    [HttpGet("ready")]
    public async Task<IActionResult> ReadinessProbe()
    {
        var (response, httpCode) = await _probeService.IsReady();

        return StatusCode(httpCode, response);
    }

    [HttpPost("ready")]
    public async Task<IActionResult> SetReadinessProbe([FromBody] ProbeConfiguration probeConfiguration)
    {
        _probeService.SetIsReady(probeConfiguration);
        var (response, httpCode) = await _probeService.IsReady();

        return StatusCode(httpCode, response);
    }

    [HttpGet("subtending_service_ready")]
    public async Task<IActionResult> TestProbe()
    {
        _logger.LogInformation("TestProbe called");
        var (response, httpCode) = await _probeService.IsSubtendingServiceReady();

        return StatusCode(httpCode, response);
    }

}
