using DataExportService.Interface;
using DataExportService.Models;
using iText.Kernel.Colors;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Kernel.Pdf;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Layout;
using iText.Kernel.Exceptions;

namespace DataExportService.Services
{
    public class PDFExportService : IExportStrategy<Conversation>
    {
        private readonly ILogger _logger;
        public PDFExportService(ILogger logger)
        {
            _logger = logger;
        }

        public string ExportData(Conversation data)
        {
            var filePath = $"conversation-{data.Id}.pdf";
            using var pdfDocument = new PdfDocument(new PdfWriter(filePath));
            using var pdf = new Document(pdfDocument);
            try
            {
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
                    pdf.Add(new Paragraph($"Date: {message.Date.ToString("yyyy-MM-dd HH:mm:ss")}\n").SetItalic().SetFontColor(ColorConstants.GRAY));

                    var lineSeparator = new LineSeparator(new SolidLine());
                    lineSeparator.SetMarginTop(10); // Ajoute une marge au-dessus de la ligne
                    lineSeparator.SetMarginBottom(10); // Ajoute une marge en dessous de la ligne

                    pdf.Add(lineSeparator);
                }

                pdf.Close();

                this._logger.LogInformation($"PDF generated successfully for conversation-{data.Id}!");
                return filePath;
            }
            catch (PdfException ex)
            {
                this._logger.LogError($"Unexpected error while generating PDF for conversation-{data.Id}: {ex.Message}");
                throw;
            }
            finally
            {
                pdf.Close();
                this._logger.LogInformation($"PDF document closed for conversation-{data.Id}.");
            }
        }
    }
}
