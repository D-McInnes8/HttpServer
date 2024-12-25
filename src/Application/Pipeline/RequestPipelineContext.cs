using System.Diagnostics.CodeAnalysis;
using Application.Request;
using Application.Routing;

namespace Application.Pipeline;

public class RequestPipelineContext
{
    public required HttpRequest Request { get; init; }
    public Route? Route { get; set; }
    
    public RequestPipelineContext()
    {
    }
    
    [SetsRequiredMembers]
    public RequestPipelineContext(HttpRequest request)
    {
        Request = request;
    }
}