
using Terminal.Gui;

namespace MagusTui;

public static class ThemeColor
{
    // Accepts names like "BrightGreen", "Black", "Cyan", etc.
    public static Color Parse(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Color.White;

        return name.Trim().ToLowerInvariant() switch
        {
            "black" => Color.Black,
            "blue" => Color.Blue,
            "green" => Color.Green,
            "cyan" => Color.Cyan,
            "red" => Color.Red,
            "magenta" => Color.Magenta,
            "brown" => Color.Brown,
            "gray" => Color.Gray,
            "darkgray" => Color.DarkGray,

            "brightblue" => Color.BrightBlue,
            "brightgreen" => Color.BrightGreen,
            "brightcyan" => Color.BrightCyan,
            "brightred" => Color.BrightRed,
            "brightmagenta" => Color.BrightMagenta,
            "brightyellow" => Color.BrightYellow,
            "white" => Color.White,
            "yellow" => Color.BrightYellow, // alias
            _ => Color.White
        };
    }
}
