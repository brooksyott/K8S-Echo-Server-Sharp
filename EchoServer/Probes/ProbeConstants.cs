namespace EchoServer.Probe;

public static class ProbeConstants
{
    // Remote service URL to get it's health
    public const string K8sEventReasonUnhealthy = "Unhealthy";
    public const string K8sEventNameLocalHealthCheck = "local-healthcheck";
    public const string K8sEventNameReadyHealthCheck = "ready-healthcheck";

    public const string K8sEventMessageIsAliveLocalFailed = "IsAlive: Local health check failed";
    public const string K8sEventMessageIsReadyFailed = "IsReady: health check failed";

    public const string K8sSubtendingServeNotReady = "Subtending service not ready";

}
