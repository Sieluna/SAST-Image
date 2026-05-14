using Domain;
using Orleans.Configuration;
using Orleans.Dashboard;
using Storage;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();

builder.UseOrleans(builder =>
{
    builder.UseAdoNetClustering(options =>
    {
        options.Invariant = "Npgsql";
        options.ConnectionString = builder.Configuration.GetConnectionString("Domain");
    });
    builder.Services.Configure<EndpointOptions>(
        builder.Configuration.GetRequiredSection("Orleans:Endpoints")
    );
    builder.AddDashboard(options => options.HideTrace = true);
    builder.Services.AddStorage(builder.Configuration);
    builder.Services.AddDomainModelJsonSerialization();
    builder.Services.AddHostedService<StorageSyncService>();
});

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapOrleansDashboard();

app.Run();
