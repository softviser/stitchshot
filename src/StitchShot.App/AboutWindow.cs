using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace StitchShot.App;

/// <summary>Version, maker, licence and the places to get help or report a bug.</summary>
public sealed class AboutWindow : Window
{
    public const string Website = "https://softviser.com.tr/stitchshot";
    public const string Repository = "https://github.com/softviser/stitchshot";

    static AboutWindow? _open;

    public static void Open()
    {
        if (_open is not null) { _open.Activate(); return; }
        _open = new AboutWindow();
        _open.Closed += (_, _) => _open = null;
        _open.Show();
    }

    AboutWindow()
    {
        Title = "About StitchShot";
        Icon = TrayIconImage.Create();
        Width = 400;
        SizeToContent = SizeToContent.Height;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        var version = typeof(AboutWindow).Assembly.GetName().Version?.ToString(3) ?? "dev";
        var links = new WrapPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };
        foreach (var (text, url) in new[] { ("Website", Website), ("Source on GitHub", Repository), ("Releases", Repository + "/releases"), ("Report an issue", Repository + "/issues") })
            links.Children.Add(new HyperlinkButton { Content = text, NavigateUri = new Uri(url), Margin = new Thickness(6, 0) });

        var help = new Button { Content = "Help" };
        help.Click += (_, _) => HelpWindow.Open();
        var close = new Button { Content = "Close", IsDefault = true, IsCancel = true };
        close.Click += (_, _) => Close();
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 10, 0, 0) };
        buttons.Children.Add(help);
        buttons.Children.Add(close);

        Content = new StackPanel
        {
            Margin = new Thickness(24),
            Spacing = 6,
            Children =
            {
                new Image { Source = TrayIconImage.Bitmap(192), Width = 96, Height = 96, Margin = new Thickness(0, 0, 0, 6) },
                new TextBlock { Text = "StitchShot", FontSize = 22, FontWeight = FontWeight.SemiBold, HorizontalAlignment = HorizontalAlignment.Center },
                new TextBlock { Text = $"Version {version}", Opacity = 0.7, HorizontalAlignment = HorizontalAlignment.Center },
                new TextBlock { Text = "Screenshot tool that stitches several captures into one tall image.", TextWrapping = TextWrapping.Wrap, TextAlignment = TextAlignment.Center, Margin = new Thickness(0, 6) },
                links,
                new TextBlock { Text = "© 2026 Softviser Yazılım · MIT license", Opacity = 0.7, FontSize = 12, HorizontalAlignment = HorizontalAlignment.Center },
                new TextBlock { Text = $"Log file: {Log.Path}", Opacity = 0.5, FontSize = 11, TextWrapping = TextWrapping.Wrap, TextAlignment = TextAlignment.Center },
                buttons,
            },
        };
    }
}
