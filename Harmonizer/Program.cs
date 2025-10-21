using Microsoft.EntityFrameworkCore;
using Harmonizer.Data;
using Harmonizer.Services.LLM;
using Harmonizer.Repositories;
using Harmonizer.Services;

var builder = WebApplication.CreateBuilder(args);

// Bind LLM options
builder.Services.Configure<LlmOptions>(builder.Configuration.GetSection("LLM"));

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Add CORS policy (adjust origins in production)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// DbContext & DI registrations
builder.Services.AddDbContext<HarmonizerDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHttpClient<ILLMClient, LmStudioLlmClient>();

// Register repositories and services for both entities
builder.Services.AddScoped<IHarmonizeRequestRepository, HarmonizeRequestRepository>();
builder.Services.AddScoped<IHarmonizedRecordRepository, HarmonizedRecordRepository>();
builder.Services.AddScoped<IHarmonizeRequestService, HarmonizeRequestService>();
builder.Services.AddScoped<IHarmonizedRecordService, HarmonizedRecordService>();
builder.Services.AddHttpClient(); // For LLM calls

var app = builder.Build();

// Apply migrations / create database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<HarmonizerDbContext>();
    db.Database.Migrate();
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