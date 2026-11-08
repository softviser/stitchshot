using System;
using System.IO;

namespace StitchShot.App;

/// <summary>Tray apps have no console, so startup and errors go to %TEMP%\StitchShot.log.</summary>
static class Log
{
    public static readonly string Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "StitchShot.log");

    public static void Write(string message)
    {
        try { File.AppendAllText(Path, $"{DateTime.Now:HH:mm:ss.fff} {message}{Environment.NewLine}"); }
        catch { /* logging must never take the app down */ }
    }
}
