using StitchShot.Core;
using Xunit;

namespace StitchShot.Core.Tests;

public class HotkeyBindingTests
{
    [Theory]
    [InlineData("PrintScreen", "PrintScreen", false, false, false)]
    [InlineData("ctrl+shift+s", "S", true, true, false)]
    [InlineData("Alt + F8", "F8", false, false, true)]
    public void Parses_modifiers_and_key(string text, string key, bool ctrl, bool shift, bool alt)
    {
        var b = HotkeyBinding.Parse(text);
        Assert.Equal(new HotkeyBinding(key, ctrl, shift, alt), b);
    }

    [Theory]
    [InlineData("Ctrl+Shift+S")]
    [InlineData("F9")]
    [InlineData("Ctrl+Alt+Win+PrintScreen")]
    public void ToString_round_trips(string text) =>
        Assert.Equal(text, HotkeyBinding.Parse(text).ToString());

    [Theory]
    [InlineData("")]
    [InlineData("Hyper+S")]
    public void Rejects_bad_input(string text) =>
        Assert.Throws<FormatException>(() => HotkeyBinding.Parse(text));
}
