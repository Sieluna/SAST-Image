using System.Text;
using Domain;
using Domain.Album.Image;
using Interface.Endpoints;
using Interface.Hubs;
using Interface.Services;
using Mediator;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Orleans.Serialization;
using Query;
using S3Storage;
using Storage;
using Storage.Images;
using Storage.Images.Queries;

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

// Register S3 as the IImageFileManager at host level
builder.Services.AddS3Storage(builder.Configuration, startRustFs: false);

builder.UseOrleansClient(client =>
{
    client.UseAdoNetClustering(options =>
    {
        options.Invariant = nameof(Npgsql);
        options.ConnectionString = client.Configuration.GetConnectionString(nameof(Domain));
    });
    client.Services.AddQuery(client.Configuration);
    client.Services.AddStorage(client.Configuration);
    // Replace local IImageFileManager with S3 in Orleans client scope
    client.Services.AddS3Storage(client.Configuration);
    client.Services.AddSerializer(b => b.AddJsonSerializer(t => t.Namespace!.StartsWith("Domain")));
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();

// SignalR Hub
app.MapHub<MainHub>("/hub");

// REST API endpoints
app.MapAuthEndpoints();
app.MapAlbumEndpoints();
app.MapCategoryEndpoints();
app.MapImageEndpoints();
app.MapUserEndpoints();

// Image file serving via IImageFileManager → S3
app.MapGet("/images/{id}", async (long id, IMediator mediator, HttpContext context) =>
{
    var actor = context.TryGetActor();
    var stream = await mediator.Send(new ImageFileQuery(new ImageId(id), ImageKind.Original, actor));
    if (stream is null)
        return Results.NotFound();
    return Results.File(stream, "image/webp");
});

await app.RunAsync();
