using System;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.TestHost;

namespace TypedSignalR.Client.Tests;

public class IntegrationTestBase
{
    internal static WebApplicationFactory<Program> WebApplicationFactory { get; private set; } = new CustomWebApplicationFactory<Program>();

    protected static HttpClient CreateClient() => WebApplicationFactory.CreateClient();

    protected static HttpMessageHandler CreateHandler() => WebApplicationFactory.Server.CreateHandler();

    protected static WebSocketClient CreateWebSocket() => WebApplicationFactory.Server.CreateWebSocketClient();

    protected static HubConnection CreateHubConnection(string path, HttpTransportType transportType)
    {
        var client = CreateClient();

        var uri = new Uri(client.BaseAddress!, path);

        if (transportType == HttpTransportType.WebSockets)
        {
            return CreateHubConnectionSkipNegotiation(uri);
        }

        return CreateHubConnectionNegotiation(uri, transportType);
    }

    private static HubConnection CreateHubConnectionSkipNegotiation(Uri uri)
    {
        var websocket = CreateWebSocket();

        var connection = new HubConnectionBuilder()
            .WithUrl(uri, options =>
            {
                options.Transports = HttpTransportType.WebSockets;
                options.SkipNegotiation = true;
                options.WebSocketFactory = async (context, cancellationToken) =>
                {
                    var client = await websocket.ConnectAsync(context.Uri, cancellationToken);
                    return client;
                };
            })
            .WithAutomaticReconnect()
            .Build();

        return connection;
    }

    private static HubConnection CreateHubConnectionNegotiation(Uri uri, HttpTransportType httpTransportType)
    {
        var handler = CreateHandler();

        var connection = new HubConnectionBuilder()
            .WithUrl(uri, options =>
            {
                options.Transports = (HttpTransportType.ServerSentEvents | HttpTransportType.LongPolling) & httpTransportType;
                options.HttpMessageHandlerFactory = _ => handler;
            })
            .WithAutomaticReconnect()
            .Build();

        return connection;
    }
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
