using Microsoft.EntityFrameworkCore;
using MessageService.Models;
using MessageService.Services;
using MessageService.Interfaces;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Ajouter le DbContext pour EF Core avec MySQL
builder.Services.AddDbContext<MessageDbContext>(options =>
{
    options.UseMySQL(Environment.GetEnvironmentVariable("MESSAGESERVICE_DB_CONNECTION")!);
});

builder.Services.AddSingleton<RabbitMQConnection>();

builder.Services.AddScoped<IProducer, ProducerService>();
builder.Services.AddScoped<MsgService>();
builder.Services.AddScoped<ConversationService>();
builder.Services.AddScoped<IGenerator<string>, JoinCodeConversationService>();

// SignalR pour la messagerie temps réel (Hub: ChatHub)
builder.Services.AddSignalR();

// CORS pour autoriser le frontend React (dev server sur le port 3000).
// AllowCredentials est requis pour les connexions WebSocket SignalR.
builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy => policy
        .WithOrigins("http://localhost:3000")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

// Ajouter les services pour Swagger UI
builder.Services.AddSwaggerGen();

// Ajouter les contr�leurs
builder.Services.AddControllers()
          .AddJsonOptions(options =>
          {
              options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
          });
     
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MessageService API v1");
    });
}

app.UseRouting();
// CORS doit être appliqué après UseRouting et avant UseAuthorization / endpoints.
app.UseCors("frontend");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Endpoint temps réel du chat (rejoindre/quitter des groupes, envoi de messages).
app.MapHub<MessageService.Hubs.ChatHub>("/hubs/chat");

app.Run();
