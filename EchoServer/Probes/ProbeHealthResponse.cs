using Microsoft.AspNetCore.Http.HttpResults;
using EchoServer.Kubernetes;

namespace EchoServer.Probe;

public class ProbeHealthResponse
{
    public String Message { get; set; }
    public PodInformation PodInformation { get; set; }
}

