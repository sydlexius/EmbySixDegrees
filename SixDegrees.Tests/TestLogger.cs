// <copyright file="TestLogger.cs" company="Six Degrees">
// Copyright © 2026 - Six Degrees Contributors. All rights reserved.
// </copyright>

namespace SixDegrees.Tests;

using System.Text;
using MediaBrowser.Model.Logging;

/// <summary>
/// A no-op logger for unit tests.
/// </summary>
public class TestLogger : ILogger
{
    public void Debug(string message, params object[] paramList)
    {
    }

    public void Debug(ReadOnlyMemory<char> message)
    {
    }

    public void Error(string message, params object[] paramList)
    {
    }

    public void Error(ReadOnlyMemory<char> message)
    {
    }

    public void ErrorException(string message, Exception exception, params object[] paramList)
    {
    }

    public void Fatal(string message, params object[] paramList)
    {
    }

    public void Fatal(ReadOnlyMemory<char> message)
    {
    }

    public void FatalException(string message, Exception exception, params object[] paramList)
    {
    }

    public void Info(string message, params object[] paramList)
    {
    }

    public void Info(ReadOnlyMemory<char> message)
    {
    }

    public void Log(LogSeverity severity, string message, params object[] paramList)
    {
    }

    public void Log(LogSeverity severity, ReadOnlyMemory<char> message)
    {
    }

    public void LogMultiline(string message, LogSeverity severity, StringBuilder additionalContent)
    {
    }

    public void Warn(string message, params object[] paramList)
    {
    }

    public void Warn(ReadOnlyMemory<char> message)
    {
    }
}
