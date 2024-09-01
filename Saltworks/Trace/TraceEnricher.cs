using System.Diagnostics;

namespace Saltworks.Trace {

    public interface TraceEnricher {

        string Enrich(TraceLogger logger, string message);
    }

    public class DateTimeEnricher : TraceEnricher {

        public string Enrich(TraceLogger logger, string message) {
            return $"[{System.DateTime.Now.ToString("HH:mm:ss:fff")}] {message}";
        }
    }

    public class MethodNameEnricher : TraceEnricher {

        public string Enrich(TraceLogger logger, string message) {
            StackFrame stackFrame = new StackFrame(4, false);
            return $"{stackFrame?.GetMethod()?.Name}:{message}";
        }
    }

    public class TypeNameEnricher : TraceEnricher {

        public string Enrich(TraceLogger logger, string message) {
            return $"{logger.type.Name}:{message}";
        }
    }

    public class ScopeEnricher : TraceEnricher {
        public string Enrich(TraceLogger logger, string message) {
            if (logger.Scopes.Count > 0)
                return $"{logger.Scopes.Peek()}:{message}";
            else
                return message;
        }
    }
}