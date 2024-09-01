using Microsoft.Extensions.Logging;

namespace Saltworks.Trace {

    public interface TraceSink {

        void Exception(string message, Exception ex);

        void Trace(string message);
    }

    public class ConsoleTraceSink : TraceSink {

        public void Exception(string message, Exception ex) {
            Console.WriteLine($"{message} - {ex.ToString() ?? ""}");
        }

        public void Trace(string message) {
            Console.WriteLine(message);
        }
    }

    public class DebugTraceSink : TraceSink {

        public void Exception(string message, Exception ex) {
            System.Diagnostics.Debug.WriteLine($"{message} - {ex.ToString() ?? ""}");
        }

        public void Trace(string message) {
            System.Diagnostics.Debug.WriteLine(message);
        }
    }

    public class LoggerTraceSink : TraceSink {
        private readonly Microsoft.Extensions.Logging.ILogger _logger;

        public LoggerTraceSink(Microsoft.Extensions.Logging.ILogger logger) {
            _logger = logger;
        }

        public void Exception(string message, Exception ex) {
            _logger.LogError(ex, message);
        }

        public void Trace(string message) {
            _logger.LogInformation(message);
        }
    }
}