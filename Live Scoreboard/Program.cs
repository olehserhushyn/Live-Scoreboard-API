using Live_Scoreboard.Hubs;
using Live_Scoreboard.Services.Generation;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

builder.Services.AddSignalR();

builder.Services.AddSingleton<IScoreGeneratorService, ScoreGeneratorService>();

// Add CORS for your React app
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Important for SignalR
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Use CORS BEFORE other middleware
app.UseCors("ReactApp");

app.UseHttpsRedirection();

// Your existing weather forecast endpoint
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapGet("start-scores", async (HttpContext context) =>
{
    // Create a scope to resolve scoped services
    var scoreGeneratorService = app.Services.GetRequiredService<IScoreGeneratorService>();
    await scoreGeneratorService.StartGeneratingScoresAsync();

    return Results.Ok("Score generation started");
});

// Map your SignalR hub
app.MapHub<ScoreHub>("/scorehub");

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

