using Microsoft.EntityFrameworkCore;
using NotificationService.BackgroundTasks;
using NotificationService.Hubs;
using NotificationService.Interfaces;
using NotificationService.Models;
using NotificationService.Services;

var builder = WebApplication.CreateBuilder(args);

// DB
builder.Services.AddDbContext<NotifDbContext>(options =>
    options.UseMySQL(builder.Configuration.GetConnectionString("DefaultConnection")));

// Services
builder.Services.AddScoped<NotificationManagerService>();
builder.Services.AddSingleton<IConsumer, ConsumerService>();
builder.Services.AddHostedService<NotificationsTask>();

// SignalR
builder.Services.AddSignalR();

// Controllers
builder.Services.AddControllers();

// Build
var app = builder.Build();

// Pipeline
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHub<NotificationHubs>("/notifications");

app.Run();
