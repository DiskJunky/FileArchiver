namespace FileArchiver.Services
{
    /// <summary>
    /// Service for application logging.
    /// Abstracts Serilog to enable testing and flexibility in logging implementation.
    /// </summary>
    public interface IApplicationLogger
    {
        /// <summary>
        /// Logs a debug-level message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        void LogDebug(string message);

        /// <summary>
        /// Logs an information-level message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        void LogInformation(string message);

        /// <summary>
        /// Logs a warning-level message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        void LogWarning(string message);

        /// <summary>
        /// Logs an error-level message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        void LogError(string message);

        /// <summary>
        /// Logs an exception with optional message.
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">Optional message context.</param>
        void LogException(Exception exception, string message = null);

        /// <summary>
        /// Closes the logger and flushes any pending writes.
        /// </summary>
        void Close();
    }
}
