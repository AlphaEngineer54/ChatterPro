namespace DataExportService.Interface
{
    /// <summary>
    /// Creator interface for exporting data in different formats. It respects the factory pattern.
    /// </summary>
    /// <typeparam name="T">The type of export strategy to be created.</typeparam>
    public interface ICreatorExport<T> where T : class
    {
        /// <summary>
        /// Create an export strategy based on the provided type and logger.
        /// </summary>
        /// <param name="type">The type of the export (e.g., "CSV", "JSON", "XML")</param>
        /// <param name="logger">The logger to be used during the export process.</param>
        /// <returns>A new instance of the export strategy of type T.</returns>
        T Create(string type, ILogger logger);
    }
}
