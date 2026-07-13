namespace DataExportService.Interface
{
    /// <summary>
    /// Interface for export strategies that define how data should be exported in different formats.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IExportStrategy<T> where T : class
    {
        /// <summary>
        /// Exports the provided data to a specified file path.
        /// </summary>
        /// <param name="data">The data to export.</param>
        /// <returns>A string representing the file path where the data was exported.</returns>
        string ExportData(T data);
    }
}
