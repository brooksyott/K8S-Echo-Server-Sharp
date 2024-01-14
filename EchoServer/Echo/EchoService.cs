using System.Collections;
using EchoServer.Kubernetes;
namespace EchoServer.Echo;


public class EchoService : IEchoService
{
    private readonly ILogger<EchoService> _logger;

    public EchoService(ILogger<EchoService> logger)
    {
        _logger = logger;
    }

    public async Task<EchoResponse> Get(HttpRequest request, HttpContext context, string requestBody = null)
    {
        EchoRequestBody echoRequestBody = new EchoRequestBody();
        echoRequestBody.Body = requestBody;

        var requestDetails = new EchoResponse
        {
            Method = request.Method,
            Host = request.Host.ToString(),
            Path = request.Path,
            Query = request.QueryString.ToString(),
            HostName = Environment.MachineName,
            RemoteIpAddress = context.Connection.RemoteIpAddress?.ToString(),
            PodInformation = new PodInformation
            {
                NodeName = Environment.GetEnvironmentVariable("NODE_NAME"),
                PodName = Environment.GetEnvironmentVariable("POD_NAME"),
                PodNamespace = Environment.GetEnvironmentVariable("POD_NAMESPACE"),
                PodIp = Environment.GetEnvironmentVariable("POD_IP")
            },
            Headers = request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
            RequestBody = echoRequestBody,
            EnvironmentVariables = Environment.GetEnvironmentVariables()
                                    .Cast<DictionaryEntry>()
                                    .ToDictionary(e => e.Key.ToString(), e => e.Value.ToString())
        };

        await Task.Delay(0);
        return requestDetails;
    }
}