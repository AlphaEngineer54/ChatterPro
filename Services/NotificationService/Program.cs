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
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "NotificationService API",
        Version = "v1",
        Description = "API for managing user notifications, including creation, retrieval, updating, and deletion of notifications for application users."
    });

    // Optional : activer les commentaires XML si disponibles
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});


// Build
var app = builder.Build();

// Activer Swagger en mode développement
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.MapHub<NotificationHubs>("/notifications");

app.Run();

