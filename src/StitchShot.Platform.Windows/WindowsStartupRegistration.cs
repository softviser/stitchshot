using System.Runtime.Versioning;
using Microsoft.Win32;
using StitchShot.Core;

namespace StitchShot.Platform.Windows;

/// <summary>Per-user Run key; no admin rights needed and it follows the exe wherever it is launched from.</summary>
[SupportedOSPlatform("windows")]
public sealed class WindowsStartupRegistration : IStartupRegistration
{
    const string KeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    const string ValueName = "StitchShot";

    public bool Enabled
    {
        get
        {
            using var key = Registry.CurrentUser.OpenSubKey(KeyPath);
            return key?.GetValue(ValueName) is string;
        }
        set
        {
            using var key = Registry.CurrentUser.CreateSubKey(KeyPath);
            if (value) key.SetValue(ValueName, $"\"{Environment.ProcessPath}\"");
            else key.DeleteValue(ValueName, throwOnMissingValue: false);
        }
    }
}
