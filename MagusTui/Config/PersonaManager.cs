using System.Globalization;

namespace MagusTui.Config;

public record PersonaInfo(string DisplayName, string RelativePath);

public static class PersonaManager
{
    private static string DefaultPersonaText =>
        "You are Tara, a playful, joyful, creative assistant with gravitas.\n" +
        "You are collaborating on MagusTui and Magus Search.\n" +
        "Be practical, kind, concise, and safety-aware.";

    public static string LoadPersonaText(MagusConfig config) =>
        LoadPersonaText(config.PersonaFile);

    public static string LoadPersonaText(string personaRelativePath)
    {
        MagusHome.Ensure();
        var path = Path.Combine(MagusHome.HomeDir, personaRelativePath);
        if (File.Exists(path))
            return File.ReadAllText(path);

        return DefaultPersonaText;
    }

    public static IReadOnlyList<PersonaInfo> ListPersonas()
    {
        MagusHome.Ensure();
        Directory.CreateDirectory(MagusHome.PersonasDir);

        var files = Directory.GetFiles(MagusHome.PersonasDir, "*.md");
        if (files.Length == 0)
        {
            var def = Path.Combine(MagusHome.PersonasDir, "persona1.md");
            if (!File.Exists(def))
                File.WriteAllText(def, DefaultPersonaText);
            files = new[] { def };
        }

        return files
            .Select(f => new PersonaInfo(ToDisplayName(f), ToRelative(f)))
            .ToList();
    }

    private static string ToDisplayName(string fullPath)
    {
        var raw = Path.GetFileNameWithoutExtension(fullPath) ?? "";
        raw = raw.Replace('-', ' ').Replace('_', ' ');
        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(raw);
    }

    private static string ToRelative(string fullPath) =>
        Path.GetRelativePath(MagusHome.HomeDir, fullPath).Replace("\\", "/");
}
