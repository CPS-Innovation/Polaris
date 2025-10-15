// <copyright file="TestLogger.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Services.Tests.TestUtilities;

using Microsoft.Extensions.Logging;

/// <summary>
/// A test logger implementation for capturing log entries during unit tests.
/// </summary>
/// <typeparam name="T">The type associated with the logger.</typeparam>
public class TestLogger<T> : ILogger<T>
{
    private readonly List<LogEntry> logs = new List<LogEntry>();

    /// <summary>
    /// Gets the captured log entries as a read-only list.
    /// </summary>
    public IReadOnlyList<LogEntry> Logs => this.logs.AsReadOnly();

    /// <summary>
    /// Begins a logical operation scope. This method is not used in this test logger.
    /// </summary>
    /// <typeparam name="TState">The type of the state to begin scope for.</typeparam>
    /// <param name="state">The state to begin scope for.</param>
    /// <returns>An IDisposable that ends the logical operation scope on disposal.</returns>
    IDisposable? ILogger.BeginScope<TState>(TState state) => null;

    /// <summary>
    /// Checks if the given log level is enabled. This implementation always returns true.
    /// </summary>
    /// <param name="logLevel">The log level to check.</param>
    /// <returns>true if the log level is enabled, otherwise false.</returns>
    public bool IsEnabled(LogLevel logLevel) => true;

    /// <summary>
    /// Logs a message with the specified log level, event ID, state, and exception.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="logLevel">The log level of the message.</param>
    /// <param name="eventId">The event ID of the log message.</param>
    /// <param name="state">The state associated with the log message.</param>
    /// <param name="exception">The exception associated with the log message, if any.</param>
    /// <param name="formatter">The function to format the state and exception into a log message.</param>
    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        var logEntry = new LogEntry
        {
            LogLevel = logLevel,
            Message = formatter(state, exception),
            Exception = exception,
        };
        this.logs.Add(logEntry);
    }

    /// <summary>
    /// Represents a log entry with log level, message, and exception information.
    /// </summary>
    public class LogEntry
    {
        /// <summary>
        /// Gets or sets the log level of the log entry.
        /// </summary>
        public LogLevel LogLevel { get; set; }

        /// <summary>
        /// Gets or sets the message of the log entry.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Gets or sets the exception associated with the log entry, if any.
        /// </summary>
        public Exception? Exception { get; set; }
    }
}
