using DataExportService.Models;
using DataExportService.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace MessagingApp_Test
{
    /// <summary>
    /// Tests des stratégies d'export (pattern Strategy) du DataExportService.
    /// L'API actuelle expose PDFExportService / CSVExportService avec ExportData(Conversation).
    /// </summary>
    public class DataExportService_Test
    {
        [Fact]
        public void PdfExport_ShouldCreatePdfFile()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var conversation = GetTestData();
            var exportService = new PDFExportService(loggerMock.Object);

            // Act
            var result = exportService.ExportData(conversation);

            // Assert
            Assert.False(string.IsNullOrEmpty(result));
            Assert.EndsWith(".pdf", result);
            Assert.True(System.IO.File.Exists(result));
            System.IO.File.Delete(result); // Nettoyage
        }

        [Fact]
        public void CsvExport_ShouldCreateCsvFile()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var conversation = GetTestData();
            var exportService = new CSVExportService(loggerMock.Object);

            // Act
            var result = exportService.ExportData(conversation);

            // Assert
            Assert.False(string.IsNullOrEmpty(result));
            Assert.EndsWith(".csv", result);
            Assert.True(System.IO.File.Exists(result));
            System.IO.File.Delete(result); // Nettoyage
        }

        private Conversation GetTestData()
        {
            return new Conversation
            {
                Id = 1,
                Title = "Discussion Test",
                Date = DateTime.Now,
                Messages = new List<Message>
                {
                    new Message
                    {
                        Id = 1,
                        Content = "Premier message de test.",
                        Date = DateTime.Now,
                        UserId = 100,
                        Status = "sent"
                    },
                    new Message
                    {
                        Id = 2,
                        Content = "Deuxième message.",
                        Date = DateTime.Now,
                        UserId = 101,
                        Status = "read"
                    }
                }
            };
        }
    }
}
