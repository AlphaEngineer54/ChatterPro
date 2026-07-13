using DataExportService.Interface;
using DataExportService.Models;

namespace DataExportService.Services
{
    /// <summary>
    /// Context that uses a specific export strategy to export data.
    /// </summary>
    public class ExportService : ICreatorExport<IExportStrategy<Conversation>>
    {
        public IExportStrategy<Conversation>? ExportStrategy { get; set; }

        public IExportStrategy<Conversation> Create(string type, ILogger logger)
        {
            return type.ToLower() switch
            {
                "csv" => new CSVExportService(logger),
                "json" => new JsonExportService(logger),
                "pdf" => new PDFExportService(logger),
                _ => throw new ArgumentException("Invalid export option. Please choose 'csv', 'json', or 'pdf'.")
            };
        }

        public string ExportData(Conversation data)
        {
            if (ExportStrategy == null)
            {
                throw new ArgumentNullException(nameof(ExportStrategy), 
                                    "Export strategy must be set before exporting data.");
            }

            try
            {
                return ExportStrategy.ExportData(data);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
