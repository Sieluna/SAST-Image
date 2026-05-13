using Domain;
using Orleans.Dashboard;
using Query;

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
    builder.AddDashboard(options => options.HideTrace = true);
    builder.Services.AddQuery(builder.Configuration);
    builder.Services.AddDomainModelJsonSerialization();
    builder.Services.AddHostedService<QuerySyncService>();
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
