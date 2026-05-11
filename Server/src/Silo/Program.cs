using Domain;
using Microsoft.EntityFrameworkCore;
using Orleans.Configuration;
using Orleans.Dashboard;
using Query;
using Storage;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.UseOrleans(builder =>
{
    builder.UseDomain();
    builder.Services.AddQuery(builder.Configuration);
    builder.Services.AddStorage(builder.Configuration);

    builder.Configure<SiloMessagingOptions>(options =>
    {
        options.ResponseTimeout = TimeSpan.FromSeconds(30);
        options.MaxRequestProcessingTime = TimeSpan.FromSeconds(30);
        options.SystemResponseTimeout = TimeSpan.FromSeconds(30);
    });

    builder.AddDashboard();
    builder.UseAdoNetClustering(options =>
    {
        options.Invariant = "Npgsql";
        options.ConnectionString = builder.Configuration.GetConnectionString("Domain");
    });
});

var app = builder.Build();

app.MapOrleansDashboard();
app.MapDefaultEndpoints();
app.UseHttpsRedirection();

await app.StartAsync();

await app.WaitForShutdownAsync();
