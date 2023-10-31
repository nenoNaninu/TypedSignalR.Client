using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace TypedSignalR.Client.Tests.InMemoryServer;

public class IntegrationTestBase
{
    internal static WebApplicationFactory<Program> WebApplicationFactory { get; private set; } = new CustomWebApplicationFactory<Program>();

    protected static HttpClient CreateClient() => WebApplicationFactory.CreateClient();

    protected static HttpMessageHandler CreateHandler() => WebApplicationFactory.Server.CreateHandler();
}

file class CustomWebApplicationFactory<TProgram>
    : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
        });

        base.ConfigureWebHost(builder);
    }
}
