namespace MessageService.Interfaces
{
    /// <summary>
    /// IGenerator interface for generating instances of type T.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IGenerator<T> where T : class
    {
        /// <summary>
        /// Generates an instance of type T with a specified length.
        /// </summary>
        /// <param name="length"></param>
        /// <returns>T - value generated</returns>
        T Generate(int length = 10);
    }
}
