namespace StitchShot.Core;

/// <summary>Whether the app starts when the user logs in. Each OS stores this differently.</summary>
public interface IStartupRegistration
{
    bool Enabled { get; set; }

    /// <summary>When the app cannot change the setting itself (MSIX package), a link that opens where the user does it; null otherwise.</summary>
    string? ExternalSettingsUri => null;
}
