using System.IO;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace StitchShot.App;

/// <summary>Shows docs/HELP.md (embedded at build time) as plain text, so help works offline and without a Markdown viewer.</summary>
public sealed class HelpWindow : Window
{
    static HelpWindow? _open;

    public static void Open()
    {
        if (_open is not null) { _open.Activate(); return; }
        _open = new HelpWindow();
        _open.Closed += (_, _) => _open = null;
        _open.Show();
    }

    HelpWindow()
    {
        Title = "StitchShot help";
        Icon = TrayIconImage.Create();
        Width = 660;
        Height = 560;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("HELP.md");
        var text = stream is null ? "Help file missing from this build." : new StreamReader(stream).ReadToEnd();
        Content = new ScrollViewer
        {
            Padding = new Thickness(16),
            Content = new SelectableTextBlock { Text = text, TextWrapping = TextWrapping.Wrap, FontFamily = new FontFamily("Consolas,Menlo,monospace"), FontSize = 13 },
        };
    }
}
