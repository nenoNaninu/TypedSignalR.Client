using System;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;

namespace TypedSignalR.Client.Tests;

public class IntegrationTestBase
{
    protected static HubConnection CreateHubConnection(string path, HttpTransportType transportType)
    {
        var uri = new Uri(new Uri("http://localhost:5105"), path);

        var connection = new HubConnectionBuilder()
            .WithUrl(uri, options =>
            {
                options.Transports = transportType;
            })
            .WithAutomaticReconnect()
            .Build();

        return connection;
    }
}
