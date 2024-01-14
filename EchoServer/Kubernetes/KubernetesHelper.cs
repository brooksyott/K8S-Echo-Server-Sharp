using k8s;
using k8s.Models;
using k8s.KubeConfigModels;
using Serilog;
using Microsoft.Extensions.Logging;
using Serilog.Core;

namespace EchoServer.Kubernetes;

public class KubernetesHelper
{

    // private readonly Logger<KubernetesHelper> _logger = (Logger<KubernetesHelper>)Log.ForContext<KubernetesHelper>();
    private V1Pod _podInformation = null;

    public KubernetesHelper()
    {

    }

    public async Task<bool> InitPodInformation()
    {
        if (_podInformation != null)
            return true;

        _podInformation = await GetPodInformation();
        Log.Warning("Namespace = {0}, {1}", _podInformation.Namespace(), _podInformation.Metadata.Namespace());
        return _podInformation == null ? false : true;
    }

    // public async Task<Eventsv1EventList> GetPodEventsAsync()
    public async Task<Corev1EventList> GetPodEventsAsync()
    {
        if (_podInformation == null)
        {
            bool podInformationRc = await InitPodInformation();
            if (podInformationRc == false)
            {
                Log.Error("GetPodEventsAsync: Could not get pod information");
                return null;
            }
        }

        try
        {
            var podName = _podInformation?.Metadata?.Name;
            var podNamespace = _podInformation?.Metadata?.Namespace();
            Log.Warning("GetPodEventsAsync: podName = {0}, podNamespace = {1}", podName, podNamespace);

            var config = KubernetesClientConfiguration.InClusterConfig();
            IKubernetes kubernetesClient = new k8s.Kubernetes(config);

            var fieldSelector = $"involvedObject.name={podName},involvedObject.kind=Pod";
            var list = await kubernetesClient.CoreV1.ListNamespacedEventAsync(podNamespace, fieldSelector: fieldSelector);
            // var list = await kubernetesClient.EventsV1.ListNamespacedEventAsync(namespaceName, fieldSelector: fieldSelector);

            return list;

        }
        catch (Exception ex)
        {
            Log.Error($"Error: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="reason"></param> ie. "Unhealthy"
    /// <param name="message"></param> ie. "Queried sub-tending service failed"
    /// <returns></returns>
    public async Task CreateKubernetesEventAsync(string reason, string message, string eventName)
    {
        if (_podInformation == null)
        {
            bool podInformationRc = await InitPodInformation();
            if (podInformationRc == false)
            {
                Log.Error("GetPodEventsAsync: Could not get pod information");
                return;
            }
        }

        // var hostName = Environment.GetEnvironmentVariable("HOSTNAME");
        var hostName = _podInformation?.Metadata?.Name;

        _podInformation = await GetPodInformation();
        var podName = _podInformation?.Metadata?.Name;
        var podNamespace = _podInformation?.Metadata?.Namespace();

        // Configure the Kubernetes client
        var config = KubernetesClientConfiguration.InClusterConfig();
        IKubernetes client = new k8s.Kubernetes(config);
        var labels = _podInformation.Metadata?.Labels;
        string app = "";
        if (labels.ContainsKey("app"))
        {
            app = labels["app"];
        }

        var @event = new Corev1Event
        {
            EventTime = DateTime.UtcNow,
            ReportingComponent = "pod", // kubelet
            ReportingInstance = _podInformation?.Spec?.NodeName,
            Reason = reason,   // ie. Unhealthy
            Action = "null",
            Message = message, // ie. Queried sub-tending service failed
            LastTimestamp = DateTime.UtcNow,
            Metadata = new V1ObjectMeta
            {
                NamespaceProperty = podNamespace,
                Name = eventName + "." + DateTime.UtcNow.Ticks,
                CreationTimestamp = DateTime.UtcNow
            },
            InvolvedObject = new V1ObjectReference
            {
                ApiVersion = "v1",
                FieldPath = $"spec.containers{{{app}}}",
                Kind = "Pod",
                Name = podName,
                Uid = _podInformation?.Metadata?.Uid,
                ResourceVersion = _podInformation?.Metadata?.ResourceVersion,
                NamespaceProperty = _podInformation?.Metadata?.Namespace()
            },
            Source = new V1EventSource
            {
                Component = "pod",
                Host = hostName
            },
            // Message = "This is a custom event from my application",
            Type = "Warning" // or "Warning"
        };

        // Send the event to the Kubernetes API server
        await client.CoreV1.CreateNamespacedEventAsync(@event, podNamespace);
    }

    public async Task<V1Pod> GetPodInformation()
    {
        // Configuring Kubernetes Client
        var config = KubernetesClientConfiguration.InClusterConfig();
        IKubernetes client = new k8s.Kubernetes(config);

        // Define the pod name and namespace
        string podName = GetPodName();
        string namespaceName = GetCurrentNamespace();

        try
        {
            // Get the pod
            var pod = await client.CoreV1.ReadNamespacedPodAsync(podName, namespaceName);
            return pod;
        }
        catch (Exception ex)
        {
            Log.Error($"GetPodInformation Error occurred: {ex.Message}");
            return null;
        }
    }

    public string GetPodName()
    {
        return Environment.GetEnvironmentVariable("HOSTNAME");
    }

    public string GetCurrentNamespace()
    {
        try
        {
            string namespacePath = "/var/run/secrets/kubernetes.io/serviceaccount/namespace";
            if (File.Exists(namespacePath))
            {
                return File.ReadAllText(namespacePath);
            }
            else
            {
                return "Default or unknown namespace";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error occurred: {ex.Message}");
            return "Error reading namespace";
        }
    }
}
