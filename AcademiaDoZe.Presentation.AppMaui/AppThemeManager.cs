using Microsoft.Maui.Controls;

namespace AcademiaDoZe.Presentation.AppMaui;

public static class AppThemeManager
{
    private sealed record Palette(
        string PageBackground,
        string CardBackground,
        string CardElevated,
        string Accent,
        string AccentStrong,
        string TextPrimary,
        string TextSecondary,
        string ErrorBackground,
        string ErrorBorder,
        string CardStroke,
        string DangerBackground,
        string DangerForeground,
        string ErrorForeground,
        string ShellForeground,
        string ShellUnselected,
        string ButtonForeground);

    private static readonly IReadOnlyDictionary<string, Palette> Palettes = new Dictionary<string, Palette>(StringComparer.Ordinal)
    {
        ["Roxo"] = new("#17141F", "#231F2E", "#2D273A", "#A78BFA", "#8B5CF6", "#F5F1FC", "#B8AFC9", "#3A2028", "#F87171", "#3A3348", "#7F1D35", "#FFFFFF", "#FECACA", "#F5F1FC", "#B8AFC9", "#FFFFFF"),
        ["Branco"] = new("#F5F4F8", "#FFFFFF", "#EEEAF4", "#8B5CF6", "#7C3AED", "#211C2B", "#6B6475", "#FDECEC", "#DC5A67", "#DED9E6", "#A61B36", "#FFFFFF", "#8B1E2D", "#211C2B", "#756D80", "#FFFFFF"),
        ["Escuro"] = new("#101114", "#1B1D22", "#262931", "#AAB4C5", "#626E82", "#F1F2F4", "#B0B4BD", "#3A2225", "#F07878", "#343841", "#802536", "#FFFFFF", "#FFD5D5", "#F1F2F4", "#B0B4BD", "#FFFFFF")
    };

    private const string PreferenceKey = "academia_do_ze_theme";
    private static string _currentTheme = "Roxo";

    public static string CurrentThemeName => _currentTheme;

    public static void Initialize()
    {
        var requestedTheme = Preferences.Default.Get(PreferenceKey, "Roxo");
        Apply(Palettes.ContainsKey(requestedTheme) ? requestedTheme : "Roxo");
    }

    public static string CycleTheme()
    {
        var themeNames = Palettes.Keys.ToArray();
        var nextIndex = (Array.IndexOf(themeNames, _currentTheme) + 1) % themeNames.Length;
        Apply(themeNames[nextIndex]);
        Preferences.Default.Set(PreferenceKey, _currentTheme);
        return _currentTheme;
    }

    private static void Apply(string themeName)
    {
        _currentTheme = themeName;
        if (Microsoft.Maui.Controls.Application.Current is not { } application)
            return;

        var palette = Palettes[themeName];
        var resources = application.Resources;
        resources["PageBackground"] = Color.FromArgb(palette.PageBackground);
        resources["CardBackground"] = Color.FromArgb(palette.CardBackground);
        resources["CardElevated"] = Color.FromArgb(palette.CardElevated);
        resources["Accent"] = Color.FromArgb(palette.Accent);
        resources["AccentStrong"] = Color.FromArgb(palette.AccentStrong);
        resources["TextPrimary"] = Color.FromArgb(palette.TextPrimary);
        resources["TextSecondary"] = Color.FromArgb(palette.TextSecondary);
        resources["ErrorBackground"] = Color.FromArgb(palette.ErrorBackground);
        resources["ErrorBorder"] = Color.FromArgb(palette.ErrorBorder);
        resources["CardStroke"] = Color.FromArgb(palette.CardStroke);
        resources["DangerBackground"] = Color.FromArgb(palette.DangerBackground);
        resources["DangerForeground"] = Color.FromArgb(palette.DangerForeground);
        resources["ErrorForeground"] = Color.FromArgb(palette.ErrorForeground);
        resources["ShellForeground"] = Color.FromArgb(palette.ShellForeground);
        resources["ShellUnselected"] = Color.FromArgb(palette.ShellUnselected);
        resources["ButtonForeground"] = Color.FromArgb(palette.ButtonForeground);
    }
}
