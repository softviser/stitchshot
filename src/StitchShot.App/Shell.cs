using System;
using System.Diagnostics;
using System.IO;

namespace StitchShot.App;

/// <summary>Opens folders and files in whatever file manager the OS has.</summary>
static class Shell
{
    public static void OpenFolder(string folder)
    {
        Directory.CreateDirectory(folder);
        if (OperatingSystem.IsWindows()) Process.Start(new ProcessStartInfo(folder) { UseShellExecute = true });
        else Run(OperatingSystem.IsMacOS() ? "open" : "xdg-open", folder);
    }

    /// <summary>Shows the file selected in its folder where the OS can; Linux file managers differ, so it opens the folder.</summary>
    public static void Reveal(string file)
    {
        if (OperatingSystem.IsWindows()) Run("explorer.exe", $"/select,\"{file}\"");
        else if (OperatingSystem.IsMacOS()) Run("open", "-R", file);
        else Run("xdg-open", Path.GetDirectoryName(file)!);
    }

    static void Run(string program, params string[] args)
    {
        var psi = new ProcessStartInfo(program) { UseShellExecute = false };
        if (OperatingSystem.IsWindows()) psi.Arguments = string.Join(' ', args); // explorer wants the odd "/select," form verbatim
        else foreach (var a in args) psi.ArgumentList.Add(a);
        Process.Start(psi);
    }
}
