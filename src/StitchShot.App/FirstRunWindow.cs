using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using StitchShot.Core;

namespace StitchShot.App;

/// <summary>Shown once, when no settings file exists yet: asks before registering the app to start with the OS.</summary>
public sealed class FirstRunWindow : Window
{
    public FirstRunWindow(Settings settings, IStartupRegistration startup)
    {
        Title = "Welcome to StitchShot";
        Icon = TrayIconImage.Create();
        Width = 420;
        SizeToContent = SizeToContent.Height;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        var yes = new Button { Content = "Yes, start with Windows", IsDefault = true };
        var no = new Button { Content = "Not now", IsCancel = true };
        yes.Click += (_, _) => { startup.Enabled = true; Done(); };
        no.Click += (_, _) => Done();

        var buttons = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6, HorizontalAlignment = HorizontalAlignment.Right };
        buttons.Children.Add(yes);
        buttons.Children.Add(no);
        Content = new StackPanel
        {
            Margin = new Thickness(16),
            Spacing = 10,
            Children =
            {
                new TextBlock { Text = "StitchShot runs in the tray and waits for its hotkeys.", TextWrapping = TextWrapping.Wrap },
                new TextBlock { Text = $"Start it automatically when you log in? You can change this later in Settings. Press {settings.RegionHotkey} any time to capture.", TextWrapping = TextWrapping.Wrap, Opacity = 0.8 },
                buttons,
            },
        };

        void Done()
        {
            settings.Save(); // the file's existence is what marks the first run as done
            Close();
        }
    }
}
