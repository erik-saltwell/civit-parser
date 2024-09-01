using Microsoft.Extensions.Logging;

namespace Saltworks.Trace {

    public class LogScope : IDisposable {
        internal TraceLogger Logger { get; set; } = default!;

        public void Dispose() {
            Logger?.Scopes.Pop();
        }
    }

    public class TraceLogger {
        public Type type = default!;

        public TraceLogger(Type type) {
            this.type = type;
        }

        private TraceLogger() {
        }

        public Stack<string> Scopes { get; } = new();

        public void Debug(string message) {
            TraceManager.Trace(this, LogLevel.Debug, message);
        }

        public void Debug<X>(string message, X item) {
            TraceManager.Trace(this, LogLevel.Debug, message, item);
        }

        public void Debug(string message, Func<string> messageFunction) {
            TraceManager.Trace(this, LogLevel.Debug, message, messageFunction);
        }

        public void Error(string message) {
            TraceManager.Trace(this, LogLevel.Error, message);
        }

        public void Error<X>(string message, X item) {
            TraceManager.Trace(this, LogLevel.Error, message, item);
        }

        public void Exception(string message, Exception ex) {
            TraceManager.Exception(this, message, ex);
        }

        public void Exception(Exception ex) {
            TraceManager.Exception(this, ex);
        }

        public void Information(string message) {
            TraceManager.Trace(this, LogLevel.Information, message);
        }

        public void Information<X>(string message, X item) {
            TraceManager.Trace(this, LogLevel.Information, message, item);
        }

        public void Information(string message, Func<string> messsageFunction) {
            TraceManager.Trace(this, LogLevel.Information, message, messsageFunction);
        }

        public LogScope Scope(string scope, bool addTrace = true) {
            Scopes.Push(scope);
            if (addTrace) Trace("Scope", scope);
            return new LogScope() { Logger = this };
        }

        public void Trace(string message) {
            TraceManager.Trace(this, LogLevel.Trace, message);
        }

        public void Trace<X>(string message, X item) {
            TraceManager.Trace(this, LogLevel.Trace, message, item);
        }

        public void Trace(string message, Func<string> messageFunction) {
            TraceManager.Trace(this, LogLevel.Trace, message, messageFunction);
        }

        public void Warning(string message) {
            TraceManager.Trace(this, LogLevel.Warning, message);
        }

        public void Warning<X>(string message, X item) {
            TraceManager.Trace(this, LogLevel.Warning, message, item);
        }
    }
}