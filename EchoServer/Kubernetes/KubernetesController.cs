namespace EchoServer.Kubernetes;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("/k8s")]
public class KubernetesController : ControllerBase
{
    private KubernetesHelper _k8sHelper = new KubernetesHelper();

    private readonly ILogger<KubernetesController> _logger;

    public KubernetesController(ILogger<KubernetesController> logger)
    {
        _logger = logger;
    }

    [HttpGet("pod/events")]
    public async Task<IActionResult> GetEvents()
    {
        var eventList = await _k8sHelper.GetPodEventsAsync();
        if (eventList == null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        return StatusCode(StatusCodes.Status200OK, eventList);
    }

    [HttpGet("pod/describe")]
    public async Task<IActionResult> GetPodInformation()
    {
        var eventList = await _k8sHelper.GetPodInformation();
        if (eventList == null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        return StatusCode(StatusCodes.Status200OK, eventList);
    }
}
