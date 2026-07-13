using DataExportService.Models;
using DataExportService.Services;
using Microsoft.AspNetCore.Mvc;

namespace DataExportService.Controllers
{
    /// <summary>
    /// Handles data export operations for user conversations in various formats (CSV, JSON, PDF).
    /// </summary>
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

        /// <summary>
        /// Exports a conversation to the specified file format.
        /// </summary>
        /// <param name="option">The export format. Supported values: "csv", "json", "pdf".</param>
        /// <param name="data">The conversation data to export.</param>
        /// <returns>Returns the exported file as a downloadable binary.</returns>
        /// <remarks>
        /// This endpoint allows exporting a conversation to a file. The supported formats are:
        /// 
        /// - CSV: `text/csv`
        /// - JSON: `application/json`
        /// - PDF: `application/pdf`
        /// 
        /// The generated file is returned as a downloadable response. Invalid or unsupported format values will result in a 400 error.
        /// 
        /// **Response Codes:**
        /// - 200: File generated and returned successfully.
        /// - 400: Bad request (invalid input or unsupported format).
        /// - 500: Internal server error during file processing.
        /// </remarks>
        [HttpPost("export-data")]
        [Produces("application/json", "text/csv", "application/pdf")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContentResult))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult ExportData([FromQuery] string option, [FromBody] Conversation data)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                this._exportService.ExportStrategy = this._exportService.Create(option, this._logger);
                var filePath = this._exportService.ExportData(data);
                var fileByte = System.IO.File.ReadAllBytes(filePath);
                var contentType = GetFileContentType(option);

                System.IO.File.Delete(filePath); // Clean up the file after reading it

                return File(fileByte, contentType, filePath);
            }
            catch (ArgumentException ex)
            {
                this._logger.LogError(ex.Message);
                return BadRequest(new { ex.Message });
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex.Message);
                return StatusCode(500, new { Message = "An internal error ocurred. Please try again later" });
            }
        }

        /// <summary>
        /// Resolves the correct MIME type based on the export format.
        /// </summary>
        /// <param name="type">Export format ("csv", "json", "pdf").</param>
        /// <returns>The corresponding MIME content type.</returns>
        private string GetFileContentType(string type)
        {
            return type == "csv" ? "text/csv" :
                   type == "json" ? "application/json" :
                    "application/pdf";
        }
    }
}