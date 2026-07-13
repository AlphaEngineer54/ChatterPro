using DataExportService.Interface;
using DataExportService.Models;
using System.Text.Json;

namespace DataExportService.Services
{
    public class JsonExportService : IExportStrategy<Conversation>
    {
       private readonly ILogger _logger;

       public JsonExportService(ILogger logger)
       {
            _logger = logger;
       }

        public string ExportData(Conversation data)
        {
            var filePath = $"conversation-{data.Id}.json";
            try
            {
                var jsonData = JsonSerializer.Serialize(data, new JsonSerializerOptions
                {
                    WriteIndented = true // For better readability
                });
                File.WriteAllText(filePath, jsonData);
                _logger.LogInformation($"JSON file for conversation-{data.Id} generated successfully!");
                return filePath;
            }
            catch (JsonException ex)
            {
                _logger.LogError($"An error occurred while generating JSON file for conversation-{data.Id}: {ex.Message}");
                throw;
            }
        }
    }
}
