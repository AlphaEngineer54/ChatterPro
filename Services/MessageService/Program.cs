using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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

// Ajouter les services pour Swagger UI
builder.Services.AddSwaggerGen();

// Ajouter les contrôleurs
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

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
