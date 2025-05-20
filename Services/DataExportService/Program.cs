using DataExportService.Services;

var builder = WebApplication.CreateBuilder(args);

// Ajouter les services dans le conteneur
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => {
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "API Export Service",
        Version = "v1",
        Description = "API pour exporter des données en PDF et CSV"
    });
});

// Ajouter le service ExportService dans le conteneur de dépendances
builder.Services.AddScoped<ExportService>();

var app = builder.Build();

// Activer Swagger uniquement en mode développement
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "API Export Service v1");
        options.RoutePrefix = string.Empty; // Accéder à Swagger via la racine
    });
}

// Configuration du pipeline des requêtes HTTP
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
