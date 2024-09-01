using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace Saltworks.Trace {

    public static class TraceManager {
        private static HashSet<Type> TraceAreas = new();
        public static LogLevel LogLevel { get; set; } = LogLevel.Information;
        private static List<TraceEnricher> Enrichers { get; } = new List<TraceEnricher>();
        private static List<TraceSink> Sinks { get; } = new List<TraceSink>();

        public static void AddArea(Type t) {
            TraceAreas.Add(t);
        }

        public static void AddSink(TraceSink sink) {
            Sinks.Add(sink);
        }

        public static void Enrich(TraceEnricher enricher) {
            Enrichers.Add(enricher);
        }

        public static bool IsEnabled(LogLevel level) {
            return level >= LogLevel.Error || level >= LogLevel;
        }

        public static TraceLogger Logger<T>() {
            return new TraceLogger(typeof(T));
        }

        internal static void Exception(TraceLogger logger, string message, Exception ex) {
            ex = ex.Demystify();
            DoException(logger, $"{message}", ex);
        }

        internal static void Exception(TraceLogger logger, Exception ex) {
            ex = ex.Demystify();
            DoException(logger, "", ex);
        }

        internal static void Trace(TraceLogger logger, LogLevel level, string message) {
            if ((TraceAreas.Contains(logger.type) || level >= LogLevel.Error) && level >= LogLevel) {
                DoTrace(logger, $"{message}");
            }
        }

        internal static void Trace<T>(TraceLogger logger, LogLevel level, string message, T item) {
            if ((TraceAreas.Contains(logger.type) || level >= LogLevel.Error) && level >= LogLevel) {
                DoTrace(logger, $"{message} - {JsonSerializer.Serialize(item)}");
            }
        }

        internal static void Trace(TraceLogger logger, LogLevel level, string message, Func<string> messageFunction) {
            if ((TraceAreas.Contains(logger.type) || level >= LogLevel.Error) && level >= LogLevel) {
                DoTrace(logger, $"{message} - {messageFunction()}");
            }
        }

        private static void DoException(TraceLogger logger, string message, Exception ex) {
            foreach (TraceEnricher enricher in Enrichers) {
                message = enricher.Enrich(logger, message);
            }
            Sinks.ForEach((sink) => sink.Exception(message, ex));
        }

        private static void DoTrace(TraceLogger logger, string message) {
            foreach (TraceEnricher enricher in Enrichers) {
                message = enricher.Enrich(logger, message);
            }
            Sinks.ForEach((sink) => sink.Trace(message));
        }
    }
}