using System.Collections;
using Microsoft.Extensions.Primitives;
using EchoServer.Kubernetes;

namespace EchoServer.Probe;

public class ProbeService : IProbeService
{
    private static Boolean _isReady = true;
    private static Boolean _isAlive = true;

    private KubernetesHelper _k8sHelper = new KubernetesHelper();

    private readonly ILogger<ProbeService> _logger;

    public ProbeService(ILogger<ProbeService> logger)
    {
        _logger = logger;
    }

    public void SetIsAlive(ProbeConfiguration probeConfiguration)
    {
        _isAlive = probeConfiguration.Enabled;
    }


    public async Task<(ProbeHealthResponse, int)> IsAlive()
    {
        // return await GetStatus(_isAlive);
        var (localHealth, localOk) = await GetLocalStatus(_isAlive);
        if (!localOk)
        {
            await _k8sHelper.CreateKubernetesEventAsync(ProbeConstants.K8sEventReasonUnhealthy, ProbeConstants.K8sEventMessageIsAliveLocalFailed, ProbeConstants.K8sEventNameLocalHealthCheck);
            return (localHealth, StatusCodes.Status500InternalServerError);
        }

        return (localHealth, StatusCodes.Status200OK);
    }

    public void SetIsReady(ProbeConfiguration probeConfiguration)
    {
        _isReady = probeConfiguration.Enabled;
    }


    public async Task<(ProbeHealthResponse, int)> IsReady()
    {
        var (healthResponse, rc) = await GetStatus(_isReady);

        if (rc != StatusCodes.Status200OK)
        {
            await _k8sHelper.CreateKubernetesEventAsync(ProbeConstants.K8sEventReasonUnhealthy, ProbeConstants.K8sEventMessageIsReadyFailed, ProbeConstants.K8sEventNameReadyHealthCheck);

        }

        return (healthResponse, rc);
    }

    private async Task<(ProbeHealthResponse, int)> GetStatus(bool returnSuccess)
    {
        var (localHealth, localOk) = await GetLocalStatus(returnSuccess);
        if (!localOk)
        {
            return (localHealth, StatusCodes.Status500InternalServerError);
        }

        var url = Environment.GetEnvironmentVariable(KubernetesConstants.SubtendingServiceEnvVarName);
        if (url == null)
        {
            return (localHealth, StatusCodes.Status200OK);
        }

        var (subtendingServiceHealth, remoteOk) = await GetSubtendingServiceStatus(url);

        if (!remoteOk)
        {
            localHealth.Message = $"{ProbeConstants.K8sSubtendingServeNotReady}: {subtendingServiceHealth?.Message}";
            _logger.LogWarning(localHealth.Message);
            return (localHealth, StatusCodes.Status500InternalServerError);
        }

        return (localHealth, StatusCodes.Status200OK);
    }

    private async Task<(ProbeHealthResponse, bool)> GetLocalStatus(bool returnSuccess)
    {
        var probeHealth = new ProbeHealthResponse
        {
            PodInformation = new PodInformation
            {
                NodeName = Environment.GetEnvironmentVariable(KubernetesConstants.NodeNameEnvVarName),
                PodName = Environment.GetEnvironmentVariable(KubernetesConstants.PodNameEnvVarName),
                PodNamespace = Environment.GetEnvironmentVariable(KubernetesConstants.PodNamespaceEnvVarName),
                PodIp = Environment.GetEnvironmentVariable(KubernetesConstants.PodIpEnvVarName)
            },
        };

        if (returnSuccess == false)
        {
            _logger.LogWarning($"GetLocalStatus: returnSuccess == false, app is not ready");
            probeHealth.Message = "NOT READY!!!";
            await Task.Delay(10);
            return (probeHealth, false);
        }

        probeHealth.Message = "Alive and ready!!!!";
        _logger.LogDebug($"GetLocalStatus: returnSuccess == true, local status is ready");
        await Task.Delay(10);
        return (probeHealth, true);
    }

    public async Task<(ProbeHealthResponse, int)> IsSubtendingServiceReady()
    {
        var url = Environment.GetEnvironmentVariable("SUBTENDING_SERVICE_IS_READY");
        if (url == null)
        {
            return (null, StatusCodes.Status404NotFound);
        }

        (ProbeHealthResponse probeHealthResponse, bool remoteOk) = await GetSubtendingServiceStatus(url);

        if (remoteOk)
        {
            _logger.LogDebug($"GetLocalStatus: returnSuccess == true, subtending service status is ready");
            return (probeHealthResponse, StatusCodes.Status200OK);
        }

        return (probeHealthResponse, StatusCodes.Status500InternalServerError);
    }

    private async Task<(ProbeHealthResponse, bool)> GetSubtendingServiceStatus(string url)
    {
        HttpResponseMessage response = null;

        // Create an instance of HttpClient
        using (var client = new HttpClient())
        {
            try
            {
                // Send a GET request to the specified URI
                response = await client.GetAsync(url);

                // Ensure we get a successful response.
                response.EnsureSuccessStatusCode();

                // Read the response content as a string
                var responseBody = await response.Content.ReadFromJsonAsync<ProbeHealthResponse>();

                // Console.WriteLine(responseBody);
                return (responseBody, true);
            }
            catch (HttpRequestException e)
            {
                _logger.LogError($"Subtending Service Failed Ready Check: {e.Message}");
                // Handle any exceptions that occur during the request
                // Console.WriteLine($"Request exception: {e.Message}");
                var probeHealthResponse = new ProbeHealthResponse
                {
                    Message = e.Message,
                    PodInformation = null
                };


                if (response == null)
                {
                    return (probeHealthResponse, false);
                }

                return (probeHealthResponse, false);
            }
        }

    }
}