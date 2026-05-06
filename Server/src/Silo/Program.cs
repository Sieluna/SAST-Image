using Domain;
using Microsoft.EntityFrameworkCore;
using Orleans.Dashboard;
using Query;
using Silo.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.UseOrleans(builder =>
{
    builder.UseDomain<IdUniquenessChecker, UsernameUniquenessChecker, CategoryExistenceChecker>();
    builder.Services.AddQueryServices(builder.Configuration);

    builder.AddDashboard();
    builder.UseRedisClustering(builder.Configuration.GetConnectionString("Redis")!);
});

var app = builder.Build();

app.MapOrleansDashboard();
app.MapDefaultEndpoints();
app.UseHttpsRedirection();

app.Run();
