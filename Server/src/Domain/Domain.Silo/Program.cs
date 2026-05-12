using System.Text;
using Domain;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Orleans.Configuration;
using Orleans.Dashboard;
using Query;
using S3Storage;
using Silo.Hubs;
using Silo.Services;
using Storage;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// JWT Authentication
var secKey = builder.Configuration.GetValue<string>("Auth:SecKey")!;
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secKey)),
            ValidateIssuer = false,
            ValidateAudience = false,
        };

        // Support access_token in query string for SignalR WebSocket connections
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hub"))
                    context.Token = accessToken;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddSignalR().AddJsonProtocol();

builder.Services.Configure<AuthOptions>(builder.Configuration.GetSection("Auth"));
builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddS3Storage(builder.Configuration, startRustFs: false);

builder.UseOrleans(builder =>
{
    builder.UseAdoNetClustering(options =>
    {
        options.Invariant = nameof(Npgsql);
        options.ConnectionString = builder.Configuration.GetConnectionString(nameof(Domain));
    });

    //builder.Services.Configure<EndpointOptions>(
    //    builder.Configuration.GetRequiredSection("Orleans:Endpoints")
    //);
    //builder.Services.Configure<SiloOptions>(
    //    builder.Configuration.GetRequiredSection("Orleans:Silo")
    //);

    builder.UseDomain();
    builder.AddDashboard(options => options.HideTrace = true);
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapOrleansDashboard();

app.MapDefaultEndpoints();
app.MapHub<MainHub>("/hub");
app.UseHttpsRedirection();

// Image file serving via S3
app.MapGet("/images/{id}", async (long id, S3StorageService storage) =>
{
    var bytes = await storage.GetBytesAsync($"images/{id}");
    if (bytes is null)
        return Results.NotFound();
    return Results.File(bytes, "image/webp");
});

await app.StartAsync();

await app.WaitForShutdownAsync();
