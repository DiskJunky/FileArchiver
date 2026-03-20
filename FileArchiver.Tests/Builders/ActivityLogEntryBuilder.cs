namespace FileArchiver.Tests.Builders
{
    /// <summary>
    /// Builder pattern for creating ActivityLogEntry instances in tests.
    /// </summary>
    public class ActivityLogEntryBuilder
    {
        private string _timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        private string _message = "Test log message";
        private bool _isError = false;
        private bool _isWarning = false;
        private bool _isSuccess = false;

        public ActivityLogEntryBuilder WithTimestamp(string timestamp)
        {
            _timestamp = timestamp;
            return this;
        }

        public ActivityLogEntryBuilder WithMessage(string message)
        {
            _message = message;
            return this;
        }

        public ActivityLogEntryBuilder AsError()
        {
            _isError = true;
            _isWarning = false;
            _isSuccess = false;
            return this;
        }

        public ActivityLogEntryBuilder AsWarning()
        {
            _isError = false;
            _isWarning = true;
            _isSuccess = false;
            return this;
        }

        public ActivityLogEntryBuilder AsSuccess()
        {
            _isError = false;
            _isWarning = false;
            _isSuccess = true;
            return this;
        }

        public ActivityLogEntryBuilder AsInfo()
        {
            _isError = false;
            _isWarning = false;
            _isSuccess = false;
            return this;
        }

        public ActivityLogEntry Build()
        {
            return new ActivityLogEntry
            {
                Timestamp = _timestamp,
                Message = _message,
                IsError = _isError,
                IsWarning = _isWarning,
                IsSuccess = _isSuccess
            };
        }

        public static ActivityLogEntryBuilder Create() => new ActivityLogEntryBuilder();
    }
}