using AuthService.Interfaces;
using AuthService.Models;
using AuthService.Repository;
using AuthService.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configuration base de données
builder.Services.AddDbContext<AuthDbContext>(options =>
{
    var connectionString = Environment.GetEnvironmentVariable("AUTHSERVICE_DB_CONNECTION")
        ?? throw new ArgumentNullException("AUTHSERVICE_DB_CONNECTION", "La chaîne de connexion à la base de données est manquante.");
    options.UseMySQL(connectionString);
});

// Services applicatifs
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<JWTService>();
builder.Services.AddScoped<IEventHandler, EventHandlerService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasherService>();

// RabbitMQ
builder.Services.AddTransient<RabbitMQConnection>(); 
builder.Services.AddScoped<IConsumer, ConsumerService>();
builder.Services.AddScoped<ProducerService>();

// BackgroundService
builder.Services.AddHostedService<EventCatchingService>();

// Swagger + MVC
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "AuthService API",
        Version = "v1",
        Description = "Authentication and user management API providing endpoints for user login, registration, token generation, and related security operations."
    });

    // Optional: Enable XML comments if you generate XML documentation from your code
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.Run();
