using Microsoft.EntityFrameworkCore;
using Harmonizer.Data;
using Harmonizer.Services.LLM;
using Harmonizer.Repositories;
using Harmonizer.Services;
using Harmonizer.Hubs;
using Harmonizer.BackgroundServices;
using System.Threading.Channels;

var builder = WebApplication.CreateBuilder(args);

// Bind LLM options
builder.Services.Configure<LlmOptions>(builder.Configuration.GetSection("LLM"));

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add SignalR
builder.Services.AddSignalR();

// Add CORS policy for SignalR
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()); // Required for SignalR
});

// Channel for background processing
builder.Services.AddSingleton(Channel.CreateUnbounded<HarmonizationJob>());

// Background service for async harmonization
builder.Services.AddHostedService<HarmonizationBackgroundService>();

// DbContext & DI registrations
builder.Services.AddDbContext<HarmonizerDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHttpClient<ILLMClient, LmStudioLlmClient>();

// Register repositories and services
builder.Services.AddScoped<IHarmonizeRequestRepository, HarmonizeRequestRepository>();
builder.Services.AddScoped<IHarmonizedRecordRepository, HarmonizedRecordRepository>();
builder.Services.AddScoped<IPromptRepository, PromptRepository>();
builder.Services.AddScoped<IWorkflowRepository, WorkflowRepository>();
builder.Services.AddScoped<IHarmonizeRequestService, HarmonizeRequestService>();
builder.Services.AddScoped<IHarmonizedRecordService, HarmonizedRecordService>();
builder.Services.AddScoped<IPromptService, PromptService>();
builder.Services.AddScoped<IWorkflowService, WorkflowService>();

// New services for enhanced harmonization platform
builder.Services.AddScoped<IDynamicTableService, DynamicTableService>();
builder.Services.AddScoped<IOutboundApiService, OutboundApiService>();
builder.Services.AddHttpClient<IApiForwardingService, ApiForwardingService>();

builder.Services.AddHttpClient(); // For LLM calls

var app = builder.Build();

// Apply migrations / create database and seed data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<HarmonizerDbContext>();
    db.Database.Migrate();
    await DataSeeder.SeedData(db);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Harmonizer API v1");
        c.RoutePrefix = string.Empty; // makes Swagger UI the root page
    });
}

// app.UseHttpsRedirection(); // Disabled for local development to avoid SSL issues
app.UseCors("AllowAll"); // enable CORS before mapping controllers
app.UseAuthorization();
app.MapControllers();
app.MapHub<DataHarmonizerHub>("/harmonizerhub"); // SignalR endpoint

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
    .WithName("GetWeatherForecast")
    .WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}