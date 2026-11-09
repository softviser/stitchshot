using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using StitchShot.Core;

namespace StitchShot.Platform.Windows;

/// <summary>
/// Inside an MSIX package the Run key is virtualised and ignored, so start-at-login is the manifest's StartupTask,
/// which only Windows Settings can switch. shortcut: no StartupTask API call, needs a Windows TFM; add it when the packaged build is the main one.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class PackagedStartupRegistration : IStartupRegistration
{
    public bool Enabled
    {
        get => false;
        set => throw new InvalidOperationException("A packaged install starts at login when it is turned on under Windows Settings > Apps > Startup.");
    }

    public string? ExternalSettingsUri => "ms-settings:startupapps";

    /// <summary>True when running from an MSIX package.</summary>
    public static bool IsPackaged()
    {
        uint length = 0;
        return GetCurrentPackageFullName(ref length, null) != 15700; // APPMODEL_ERROR_NO_PACKAGE
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    static extern int GetCurrentPackageFullName(ref uint packageFullNameLength, char[]? packageFullName);
}
