using DataExportService.Models;
using DataExportService.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Eventing.Reader;

namespace DataExportService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataExportController : ControllerBase
    {
        private readonly ExportService _exportService;
        private readonly ILogger<DataExportController> _logger;

        public DataExportController(ExportService exportService, ILogger<DataExportController> logger)
        {
            this._exportService = exportService;
            this._logger = logger;
        }

        [HttpPost("export-data")]
        public IActionResult ExportData([FromQuery] string option, [FromBody] Conversation data)
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                switch (option.ToLower())
                {
                    case "pdf":
                        this._exportService.GeneratePdf(data);
                        break;
                    case "csv":
                        this._exportService.GenerateCSV(data);
                        break;
                    default:
                        return BadRequest(new { Message = "Please select an valid option between 'pdf' or 'csv' to export your data" });
                }

                var filePath = $"conversation-{data.Id}.{option}";
                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                var contentType = option.ToLower() == "pdf" ? "application/pdf" : "text/csv";

                // Supprime le fichier temporaire après lecture
                System.IO.File.Delete(filePath);

                return File(fileBytes, contentType, filePath);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex.Message);
                return StatusCode(500, new { Message = "An internal error ocurred. Please try again later" });
            }
        }
    }
}
