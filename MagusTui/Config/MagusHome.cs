namespace MagusTui.Config;

public static class MagusHome
{
    public static string HomeDir =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".magus");

    public static string ConfigPath => Path.Combine(HomeDir, "config.json");
    public static string PersonasDir => Path.Combine(HomeDir, "personas");
    public static string ThemesDir => Path.Combine(HomeDir, "themes");
    public static string MemoryDir => Path.Combine(HomeDir, "memory");

    public static void Ensure()
    {
        Directory.CreateDirectory(HomeDir);
        Directory.CreateDirectory(PersonasDir);
        Directory.CreateDirectory(ThemesDir);
        Directory.CreateDirectory(MemoryDir);
    }
}
