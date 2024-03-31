using AspNetCore.SignalR.OpenTelemetry;
using TypedSignalR.Client.Tests.Server.Hubs;
using TypedSignalR.Client.Tests.Server.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSignalR()
    .AddHubInstrumentation();

builder.Services.AddSingleton<IDataStore, DataStore>();

var app = builder.Build();

// Configure the HTTP request pipeline.

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHub<UnaryHub>("/Hubs/UnaryHub");
app.MapHub<SideEffectHub>("/Hubs/SideEffectHub");
app.MapHub<ReceiverTestHub>("/Hubs/ReceiverTestHub");
app.MapHub<StreamingHub>("/Hubs/StreamingHub");
app.MapHub<ClientResultsTestHub>("/Hubs/ClientResultsTestHub");
app.MapHub<InheritTestHub>("/Hubs/InheritTestHub");
app.MapHub<InheritReceiverTestHub>("/Hubs/InheritReceiverTestHub");
app.MapHub<NullableTestHub>("/Hubs/NullableTestHub");

app.Run();
