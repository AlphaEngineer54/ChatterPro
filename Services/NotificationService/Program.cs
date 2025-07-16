using Microsoft.EntityFrameworkCore;
using NotificationService.BackgroundTasks;
using NotificationService.Hubs;
using NotificationService.Interfaces;
using NotificationService.Models;
using NotificationService.Services;

var builder = WebApplication.CreateBuilder(args);

// DB
builder.Services.AddDbContext<NotifDbContext>(options =>
    options.UseMySQL(Environment.GetEnvironmentVariable("NOTIFICATIONSERVICE_DB_CONNECTION")!));

// Services
builder.Services.AddScoped<NotificationManagerService>();
builder.Services.AddSingleton<IConsumer, ConsumerService>();
builder.Services.AddHostedService<NotificationsTask>();

// SignalR
builder.Services.AddSignalR();

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Build
var app = builder.Build();

// Pipeline
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHub<NotificationHubs>("/notifications");

// Activer Swagger en mode développement
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();
