using System.Collections;
using k8s.Models;
namespace EchoServer.Probe;


public interface IProbeService
{
    void SetIsAlive(ProbeConfiguration probeConfiguration);
    Task<(ProbeHealthResponse, int)> IsAlive();
    void SetIsReady(ProbeConfiguration probeConfiguration);
    Task<(ProbeHealthResponse, int)> IsReady();
    Task<(ProbeHealthResponse, int)> GetStatusSubtendingService();
    // Task<Eventsv1EventList> GetPodEventsAsync();
}