using Application.Response;

namespace Application.Pipeline.StaticFiles;

public class StaticFileRequestHandler : IRequestHandler
{
    public Task<HttpResponse> HandleAsync(RequestPipelineContext ctx)
    {
        var staticFileRoute = ctx.GetData<StaticFileRoute>();
        if (staticFileRoute is null)
        {
            return Task.FromResult(HttpResponse.InternalServerError());
        }

        if (staticFileRoute.IsDirectory)
        {
            throw new NotImplementedException();
        }
        else
        {
            var data = File.ReadAllText(staticFileRoute.PhysicalPath);
            return Task.FromResult(HttpResponse.Ok(data));
        }
    }
}