using CsvHelper;
using DataExportService.Models;
using iText.Kernel.Colors;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using System.Globalization;

namespace DataExportService.Services
{
    public class ExportService
    {

        private readonly ILogger<ExportService> _logger;

        public ExportService(ILogger<ExportService> logger)
        {
            _logger = logger;
        }

        public string GeneratePdf(Conversation data)
        {
            var filePath = $"conversation-{data.Id}.pdf";
            using var pdfDocument = new PdfDocument(new PdfWriter(filePath));
            try
            {
                var pdf = new Document(pdfDocument);

                var title = new Paragraph($"{data.Title}")
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetFontSize(18)
                            .SetBold();
                pdf.Add(title);

                pdf.Add(new Paragraph("\n"));

                pdf.Add(new Paragraph($"Creation date of the conversation: {data.Date.ToShortDateString()}")
                           .SetFontSize(12)
                           .SetItalic()
                           .SetFontColor(ColorConstants.GRAY));
                pdf.Add(new Paragraph("Messages:"));

                foreach (var message in data.Messages)
                {
                    pdf.Add(new Paragraph($"User ID: {message.UserId}\n").SetBold());
                    pdf.Add(new Paragraph($"Message: {message.Content}\n").SetFontColor(ColorConstants.BLACK));
                    pdf.Add(new Paragraph($"Date: {message.Date.ToShortDateString()}\n").SetItalic().SetFontColor(ColorConstants.GRAY));

                    var lineSeparator = new LineSeparator(new SolidLine());
                    lineSeparator.SetMarginTop(10); // Ajoute une marge au-dessus de la ligne
                    lineSeparator.SetMarginBottom(10); // Ajoute une marge en dessous de la ligne

                    pdf.Add(lineSeparator);
                }
                
                pdf.Close();
           
                this._logger.LogInformation($"PDF generated successfully for conversation-{data.Id}!");
                return filePath;
            }
            catch (Exception ex)
            {
                this._logger.LogError($"Unexpected error while generating PDF for conversation-{data.Id}: {ex.Message}");
                return string.Empty;
            }
        }

        public void GenerateCSV(Conversation data)
        {
            try
            {
                var filePath = $"conversation-{data.Id}.csv";
                using var writer = new StreamWriter(filePath);
                using (var csvWritter = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    foreach (var message in data.Messages)
                    {
                        csvWritter.WriteRecords(new[] { message });
                        csvWritter.NextRecord();
                    }
                }

                this._logger.LogInformation($"CSV file for conversation-{data.Id} generated successfully!");
            }
            catch(CsvHelperException ex)
            {
                this._logger.LogError($"An error occured while generating CSV file for conversation-{data.Id}: {ex.Message}");
            }
        }
    }
}
