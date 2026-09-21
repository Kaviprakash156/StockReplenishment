using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using StockReplenishment.Api.Middleware;
using StockReplenishment.Application.Interfaces;
using StockReplenishment.Application.Services;
using StockReplenishment.Infrastructure.BackgroundServices;
using StockReplenishment.Infrastructure.Data;
using StockReplenishment.Infrastructure.Repositories;
using StockReplenishment.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Services

builder.Services.AddControllers();

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "Stock Replenishment API",
            Version = "v1/swagger.json",
            Description = "REST API for the Stock Replenishment Request System"
        });
});


// Database

builder.Services.AddDbContext<StockReplenishmentDbContext>(options =>
{
    options.UseInMemoryDatabase("StockReplenishmentDb");
});

// Application Services

builder.Services.AddScoped<
    IReplenishmentRequestService,
    ReplenishmentRequestService>();

// Infrastructure Services

builder.Services.AddScoped<
    IReplenishmentRequestRepository,
    ReplenishmentRequestRepository>();

builder.Services.AddSingleton<
    IStockValidationQueue,
    StockValidationQueue>();

builder.Services.AddScoped<
    IStockAvailabilityService,
    SimulatedStockAvailabilityService>();

// Background Worker

builder.Services.AddHostedService<
    StockValidationBackgroundService>();

// Build Application

var app = builder.Build();

// Seed Database

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider
        .GetRequiredService<StockReplenishmentDbContext>();

    await DbSeeder.SeedAsync(dbContext);
}

// HTTP Request Pipeline

app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(options =>
    {
        // Explicitly generate OpenAPI 3.0
        options.OpenApiVersion = OpenApiSpecVersion.OpenApi2_0;
    });


    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "Stock Replenishment API v1");

        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();