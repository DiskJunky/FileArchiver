using System;
using Serilog;

namespace FileArchiver.Services
{
    /// <summary>
    /// Application logger implementation wrapping Serilog.
    /// Provides abstraction over logging to enable testing and flexibility.
    /// </summary>
    public class ApplicationLogger : IApplicationLogger
    {
        /// <summary>
        /// Logs a debug message.
        /// </summary>
        public void LogDebug(string message)
        {
            Log.Debug(message);
        }

        /// <summary>
        /// Logs an information message.
        /// </summary>
        public void LogInformation(string message)
        {
            Log.Information(message);
        }

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        public void LogWarning(string message)
        {
            Log.Warning(message);
        }

        /// <summary>
        /// Logs an error message.
        /// </summary>
        public void LogError(string message)
        {
            Log.Error(message);
        }

        /// <summary>
        /// Logs an exception.
        /// </summary>
        public void LogException(Exception exception, string message = null)
        {
            if (message != null)
            {
                Log.Error(exception, message);
            }
            else
            {
                Log.Error(exception, "An exception occurred");
            }
        }

        /// <summary>
        /// Closes the logger.
        /// </summary>
        public void Close()
        {
            Log.CloseAndFlush();
        }
    }
}
