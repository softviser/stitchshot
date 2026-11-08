using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace StitchShot.App;

/// <summary>Small icon-only buttons used by the overlay toolbar and the HUD. Paths are Material Design Icons (Apache 2.0).</summary>
static class IconButton
{
    public const string Save = "M15,9H5V5H15M12,19A3,3 0 0,1 9,16A3,3 0 0,1 12,13A3,3 0 0,1 15,16A3,3 0 0,1 12,19M17,3H5C3.89,3 3,3.9 3,5V19A2,2 0 0,0 5,21H19A2,2 0 0,0 21,19V7L17,3Z";
    public const string Copy = "M19,21H8V7H19M19,5H8A2,2 0 0,0 6,7V21A2,2 0 0,0 8,23H19A2,2 0 0,0 21,21V7A2,2 0 0,0 19,5M16,1H4A2,2 0 0,0 2,3V17H4V3H16V1Z";
    public const string Close = "M19,6.41L17.59,5L12,10.59L6.41,5L5,6.41L10.59,12L5,17.59L6.41,19L12,13.41L17.59,19L19,17.59L13.41,12L19,6.41Z";
    public const string Camera = "M4,4H7L9,2H15L17,4H20A2,2 0 0,1 22,6V18A2,2 0 0,1 20,20H4A2,2 0 0,1 2,18V6A2,2 0 0,1 4,4M12,7A5,5 0 0,0 7,12A5,5 0 0,0 12,17A5,5 0 0,0 17,12A5,5 0 0,0 12,7M12,9A3,3 0 0,1 15,12A3,3 0 0,1 12,15A3,3 0 0,1 9,12A3,3 0 0,1 12,9Z";
    public const string PageDown = "M16.59,5.59L18,7L12,13L6,7L7.41,5.59L12,10.17L16.59,5.59M16.59,11.59L18,13L12,19L6,13L7.41,11.59L12,16.17L16.59,11.59Z";
    public const string Undo = "M12.5,8C9.85,8 7.45,9 5.6,10.6L2,7V16H11L7.38,12.38C8.77,11.22 10.54,10.5 12.5,10.5C16.04,10.5 19.05,12.81 20.1,16L22.47,15.22C21.08,11.03 17.15,8 12.5,8Z";
    public const string Select = "M4,2H6V4H4V2M8,2H10V4H8V2M12,2H14V4H12V2M16,2H18V4H16V2M20,2H22V4H20V2M4,6H6V8H4V6M4,10H6V12H4V10M4,14H6V16H4V14M4,18H6V20H4V18M20,6H22V8H20V6M20,10H22V12H20V10M20,14H22V16H20V14M20,18H22V20H20V18M8,18H10V20H8V18M12,18H14V20H12V18M16,18H18V20H16V18Z";
    public const string Check = "M21,7L9,19L3.5,13.5L4.91,12.09L9,16.17L19.59,5.59L21,7Z";
    public const string Layers = "M3,3H11V11H3V3M13,3H21V11H13V3M3,13H11V21H3V13M13,13H21V21H13V13Z";

    public static readonly IBrush PanelBrush = new SolidColorBrush(Color.FromArgb(0xE6, 0x20, 0x20, 0x20));

    public static Button Create(string iconPath, string tooltip, Action action)
    {
        var button = new Button
        {
            Content = new PathIcon { Data = Geometry.Parse(iconPath), Width = 14, Height = 14 },
            Padding = new Thickness(5),
            MinWidth = 0,
            MinHeight = 0,
            Background = Brushes.Transparent,
            Foreground = Brushes.White, // panels are dark whatever the OS theme says
        };
        ToolTip.SetTip(button, tooltip);
        ToolTip.SetShowDelay(button, 300);
        button.Click += (_, _) => action();
        return button;
    }
}
