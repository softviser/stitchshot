namespace StitchShot.Core;

/// <summary>Whether the app starts when the user logs in. Each OS stores this differently.</summary>
public interface IStartupRegistration
{
    bool Enabled { get; set; }
}
