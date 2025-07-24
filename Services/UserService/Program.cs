using Microsoft.EntityFrameworkCore;
using UserService.Hubs;
using UserService.Interfaces;
using UserService.Models;
using UserService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<UserDbContext>(options =>
{
    var connectionString = Environment.GetEnvironmentVariable("USERSERVICE_DB_CONNECTION")!;
    options.UseMySQL(connectionString);
});

// Injecter les dépendances au conteneur
builder.Services.AddScoped<IProducer, ProducerService>();
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<IEventHandler, MultiEventHandler>();

builder.Services.AddSingleton<RabbitMQConnection>();
builder.Services.AddSingleton<IConsumer, ConsumerService>();

// Injecter les services asynchrones
builder.Services.AddHostedService<ConsumerBackgroundService>();

builder.Services.AddSignalR();

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

// Configure the HTTP request pipeline.
app.UseRouting();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapHub<UserHub>("/userHub");
app.MapControllers();

app.Run();