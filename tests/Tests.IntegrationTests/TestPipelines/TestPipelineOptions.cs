using HttpServer.Pipeline;

namespace Tests.IntegrationTests.TestPipelines;

public class TestPipelineOptions(IServiceProvider services) : RequestPipelineBuilderOptions(services)
{
    public bool UseTestPlugin { get; set; }
}