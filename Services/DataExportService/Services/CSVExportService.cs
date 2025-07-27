using CsvHelper;
using DataExportService.Interface;
using DataExportService.Models;
using System.Globalization;

namespace DataExportService.Services
{
    public class CSVExportService : IExportStrategy<Conversation>
    {
        private readonly ILogger _logger;

        public CSVExportService(ILogger logger)
        {
            _logger = logger;
        }

        public string ExportData(Conversation data)
        {
            var filePath = $"conversation-{data.Id}.csv";
            using var writer = new StreamWriter(filePath);
            try
            {
                using var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture);

                // Préparer les lignes à écrire
                var rows = new List<ConversationRowCSV>();

                // Lignes messages associées
                rows.AddRange(data.Messages.Select(m => new ConversationRowCSV
                {
                    ConversationId = data.Id,
                    ConversationTitle = data.Title,
                    ConversationDate = data.Date,
                    MessageId = m.Id,
                    MessageContent = m.Content,
                    MessageDate = m.Date,
                    MessageUserId = m.UserId,
                    MessageStatus = m.Status
                }));

                // Ecrire toutes les lignes en une fois
                csvWriter.WriteRecords(rows);

                this._logger.LogInformation($"CSV file for conversation-{data.Id} generated successfully!");
                return filePath;
            }
            catch (CsvHelperException ex)
            {
                this._logger.LogError($"An error occured while generating CSV file for conversation-{data.Id}: {ex.Message}");
                throw;
            }
            finally
            {
                writer.Close();
                this._logger.LogInformation($"CSV file closed for conversation-{data.Id}.");
            }
        }
    }
}
