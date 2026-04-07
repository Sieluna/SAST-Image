using System.Text.Json;
using Infrastructure;
using WebAPI.Exceptions;
using WebAPI.Utilities;

var builder = WebApplication.CreateBuilder(args);

builder.AddServices();
builder.Services.AddExceptionHandlers();
builder.Services.AddResponseCaching();
builder.Logging.AddLogger();
builder
    .Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new JsonStringLongConverter());
    });

var app = builder.Build();

app.RunBackend();
