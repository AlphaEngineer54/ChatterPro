using DataExportService.Models;
using DataExportService.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace MessagingApp_Test
{
    public class DataExportService_Test
    {
        [Fact]
        public void ShouldReturnThePDFFilePath()
        { 
            // Arrange
            var loggerMock = new Mock<ILogger<ExportService>>();
            var conversation = GetTestData();
            var exportService = new ExportService(loggerMock.Object);

            // Act
            var result = exportService.GeneratePdf(conversation);

            // Assert
            Assert.True(!string.IsNullOrEmpty(result));
            Assert.True(result.EndsWith(".pdf"));
            Assert.True(System.IO.File.Exists(result));
            System.IO.File.Delete(result); // Clean up the file after test
        }

        [Fact]
        public void ShouldReturnTheCSVFile()
        {
            // Arrange
            var logger = new Mock<ILogger<ExportService>>();
            var conversation = GetTestData();
            var exportService = new ExportService(logger.Object);
            var filePath = $"conversation-{conversation.Id}.csv";

            // Act
            var exception = Record.Exception(() => exportService.GenerateCSV(conversation));

            // Assert
            Assert.True(System.IO.File.Exists(filePath));
            Assert.True(filePath.EndsWith(".csv"));
            Assert.Null(exception); 
            System.IO.File.Delete(filePath); // Clean up the file after test
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
