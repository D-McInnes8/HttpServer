using BenchmarkDotNet.Attributes;
using HttpServer;
using HttpServer.Response;
using HttpServer.Routing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Benchmarks;

public class RequestBenchmarks
{
    private IHttpWebServer? _httpWebServer;
    private readonly string _data = new string('a', 1024 * 4048);
    
    [GlobalSetup]
    public async Task Setup()
    {
        var builder = HttpWebServer.CreateBuilder(0);
        builder.Services.RemoveAll<ILoggerProvider>();
        _httpWebServer = builder.Build();
        _httpWebServer.MapGet("/", _ => HttpResponse.Ok());
        _httpWebServer.MapPost("/", _ => HttpResponse.Ok());
        _httpWebServer.MapGet("/large", _ => HttpResponse.Ok(_data));
        await _httpWebServer.StartAsync();
    }
    
    [Benchmark(Baseline = true)]
    public async Task EmptyRequest()
    {
        using var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri($"http://localhost:{_httpWebServer!.Port}");
        await httpClient.GetAsync("/");
    }
    
    [Benchmark]
    public async Task InvalidRoute()
    {
        using var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri($"http://localhost:{_httpWebServer!.Port}");
        await httpClient.GetAsync("/invalid");
    }

    [Benchmark]
    public async Task LargeRequest()
    {
        using var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri($"http://localhost:{_httpWebServer!.Port}");
        await httpClient.PostAsync("/", new StringContent(_data));
    }
    
    [Benchmark]
    public async Task LargeResponse()
    {
        using var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri($"http://localhost:{_httpWebServer!.Port}");
        await httpClient.GetAsync("/large");
    }
}