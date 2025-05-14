using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configuration de Ocelot
builder.Configuration.AddJsonFile("ocelot-configuration.json", optional: false, reloadOnChange: true);

// Configuration des services
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "http://localhost:5000",
            ValidAudience = "messaging_api",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("5f2d6ab9e4c8210fd8c7a3f91b6e72adc4f9137e25ab8d3c6eaf7b014f29c3db"))
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync("{\"message\": \"Token invalide ou expiré.\"}");
            }
        };
    });


builder.Services.AddOcelot();

var app = builder.Build();

// Middleware Ocelot
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
await app.UseOcelot();

app.Run();
