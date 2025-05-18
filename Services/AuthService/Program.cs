using AuthService.Interfaces;
using AuthService.Models;
using AuthService.Repository;
using AuthService.Services;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Security;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<AuthDbContext>(options =>
{
    // Récupération de la chaîne de connexion depuis les variables d'environnement ou le fichier de configuration
    var connectionString = builder.Configuration.GetConnectionString("AuthServiceDB");
    options.UseMySQL(connectionString);
});

builder.Services.AddSwaggerGen();

// Ajouter les services de contrôle
builder.Services.AddControllers();

// Injecter les dépendances de classe 
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ProducerService>();
builder.Services.AddScoped<JWTService>();
builder.Services.AddScoped<IEventHandler, EventHandlerService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasherService>();

builder.Services.AddSingleton<IConsumer, ConsumerService>();
builder.Services.AddSingleton<RabbitMQConnection>();

builder.Services.AddHostedService<EventCatchingService>();

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
app.UseAuthorization();

app.MapControllers();

app.Run();
