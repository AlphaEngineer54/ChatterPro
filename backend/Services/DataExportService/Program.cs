using DataExportService.Services;

var builder = WebApplication.CreateBuilder(args);

// Ajouter les services dans le conteneur
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "DataExport API",
        Version = "v1",
        Description = "This API provides endpoints for exporting conversation data in various formats such as CSV, JSON, and PDF. It allows clients to submit conversation data and receive export files accordingly."
    });


    // Optional: Enable XML comments if you generate XML documentation from your code
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
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
app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.Run();
