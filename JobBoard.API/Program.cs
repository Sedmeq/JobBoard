using JobBoard.API;
using JobBoard.API.Extensions;
using JobBoard.API.Middleware;
using JobBoard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Text;

// .env faylını yüklə (varsa). Dəyərlər mövcud mühit dəyişənlərini əvəz etmir,
// lakin appsettings.json-u override edir (default AddEnvironmentVariables vasitəsilə).
DotEnv.Load();

var builder = WebApplication.CreateBuilder(args);

// Ensure console uses UTF-8 so log messages with non-ASCII characters render correctly
Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
builder.Host.UseSerilog();

// Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddValidation();
builder.Services.AddSwaggerWithJwt();
builder.Services.AddRateLimiting();
builder.Services.AddCorsPolicy(builder.Configuration);

builder.Services.AddSignalR();

builder.Services.AddAuthorization();

var app = builder.Build();

// Middleware pipeline
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Seed Data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await SeedData.SeedAsync(db);
}

app.UseCors("AllowFrontend");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();
app.MapControllers();
app.MapHub<JobBoard.API.Hubs.NotificationHub>("/hubs/notifications");

app.Run();