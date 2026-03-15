namespace BlazeUI.UI.CLI.Themes;

/// <summary>
/// An OKLCH gray scale with 11 shades (50–950), matching shadcn/ui v4's base color presets.
/// Each base color gives the UI a different undertone: warm (stone, taupe), cool (zinc, mist),
/// green (olive), purple (mauve), or pure achromatic (neutral).
/// </summary>
/// <remarks>
/// The shade values are sourced from shadcn/ui v4's theme registry, not directly from Tailwind's
/// color scales (the two differ slightly in chroma and hue at some positions).
/// </remarks>
internal sealed record BaseColorScale(
    string Name,
    string S50, string S100, string S200, string S300, string S400,
    string S500, string S600, string S700, string S800, string S900, string S950)
{
    internal static readonly BaseColorScale Neutral = new(
        "neutral",
        "oklch(0.985 0 0)", "oklch(0.97 0 0)", "oklch(0.922 0 0)", "oklch(0.87 0 0)",
        "oklch(0.708 0 0)", "oklch(0.556 0 0)", "oklch(0.439 0 0)", "oklch(0.371 0 0)",
        "oklch(0.269 0 0)", "oklch(0.205 0 0)", "oklch(0.145 0 0)");

    internal static readonly BaseColorScale Stone = new(
        "stone",
        "oklch(0.985 0.001 106.423)", "oklch(0.97 0.001 106.424)", "oklch(0.923 0.003 48.717)", "oklch(0.869 0.005 56.366)",
        "oklch(0.709 0.01 56.259)", "oklch(0.553 0.013 58.071)", "oklch(0.444 0.011 73.639)", "oklch(0.374 0.01 67.558)",
        "oklch(0.268 0.007 34.298)", "oklch(0.216 0.006 56.043)", "oklch(0.147 0.004 49.25)");

    internal static readonly BaseColorScale Zinc = new(
        "zinc",
        "oklch(0.985 0 0)", "oklch(0.967 0.001 286.375)", "oklch(0.92 0.004 286.32)", "oklch(0.871 0.006 286.286)",
        "oklch(0.705 0.015 286.067)", "oklch(0.552 0.016 285.938)", "oklch(0.442 0.017 285.786)", "oklch(0.37 0.013 285.805)",
        "oklch(0.274 0.006 286.033)", "oklch(0.21 0.006 285.885)", "oklch(0.141 0.005 285.823)");

    internal static readonly BaseColorScale Mauve = new(
        "mauve",
        "oklch(0.985 0 0)", "oklch(0.96 0.003 325.6)", "oklch(0.922 0.005 325.62)", "oklch(0.865 0.012 325.68)",
        "oklch(0.711 0.019 323.02)", "oklch(0.542 0.034 322.5)", "oklch(0.435 0.029 321.78)", "oklch(0.364 0.029 323.89)",
        "oklch(0.263 0.024 320.12)", "oklch(0.212 0.019 322.12)", "oklch(0.145 0.008 326)");

    internal static readonly BaseColorScale Olive = new(
        "olive",
        "oklch(0.988 0.003 106.5)", "oklch(0.966 0.005 106.5)", "oklch(0.93 0.007 106.5)", "oklch(0.88 0.011 106.6)",
        "oklch(0.737 0.021 106.9)", "oklch(0.58 0.031 107.3)", "oklch(0.466 0.025 107.3)", "oklch(0.394 0.023 107.4)",
        "oklch(0.286 0.016 107.4)", "oklch(0.228 0.013 107.4)", "oklch(0.153 0.006 107.1)");

    internal static readonly BaseColorScale Mist = new(
        "mist",
        "oklch(0.987 0.002 197.1)", "oklch(0.963 0.002 197.1)", "oklch(0.925 0.005 214.3)", "oklch(0.872 0.007 219.6)",
        "oklch(0.723 0.014 214.4)", "oklch(0.56 0.021 213.5)", "oklch(0.45 0.017 213.2)", "oklch(0.378 0.015 216)",
        "oklch(0.275 0.011 216.9)", "oklch(0.218 0.008 223.9)", "oklch(0.148 0.004 228.8)");

    internal static readonly BaseColorScale Taupe = new(
        "taupe",
        "oklch(0.986 0.002 67.8)", "oklch(0.96 0.002 17.2)", "oklch(0.922 0.005 34.3)", "oklch(0.868 0.007 39.5)",
        "oklch(0.714 0.014 41.2)", "oklch(0.547 0.021 43.1)", "oklch(0.438 0.017 39.3)", "oklch(0.367 0.016 35.7)",
        "oklch(0.268 0.011 36.5)", "oklch(0.214 0.009 43.1)", "oklch(0.147 0.004 49.3)");

    internal static IReadOnlyList<BaseColorScale> All { get; } = [Neutral, Stone, Zinc, Mauve, Olive, Mist, Taupe];

    internal static BaseColorScale? Find(string name) =>
        All.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Generates the <c>:root</c> (light) and <c>.dark</c> CSS variable blocks for this base color,
    /// optionally overlaying an accent color for primary/chart/sidebar-primary variables.
    /// </summary>
    internal string GenerateThemeCss(AccentColor? accent = null)
    {
        // Base color values — used unless overridden by accent.
        var lPrimary = accent?.LightPrimary ?? S900;
        var lPrimaryFg = accent?.LightPrimaryForeground ?? S50;
        var lSecondary = accent?.LightSecondary ?? S100;
        var lSecondaryFg = accent?.LightSecondaryForeground ?? S900;
        var lChart1 = accent?.LightChart1 ?? S300;
        var lChart2 = accent?.LightChart2 ?? S500;
        var lChart3 = accent?.LightChart3 ?? S600;
        var lChart4 = accent?.LightChart4 ?? S700;
        var lChart5 = accent?.LightChart5 ?? S800;
        var lSidebarPrimary = accent?.LightSidebarPrimary ?? S900;
        var lSidebarPrimaryFg = accent?.LightSidebarPrimaryForeground ?? S50;

        var dPrimary = accent?.DarkPrimary ?? S200;
        var dPrimaryFg = accent?.DarkPrimaryForeground ?? S900;
        var dSecondary = accent?.DarkSecondary ?? S800;
        var dSecondaryFg = accent?.DarkSecondaryForeground ?? S50;
        var dChart1 = accent?.DarkChart1 ?? S300;
        var dChart2 = accent?.DarkChart2 ?? S500;
        var dChart3 = accent?.DarkChart3 ?? S600;
        var dChart4 = accent?.DarkChart4 ?? S700;
        var dChart5 = accent?.DarkChart5 ?? S800;
        var dSidebarPrimary = accent?.DarkSidebarPrimary ?? "oklch(0.488 0.243 264.376)";
        var dSidebarPrimaryFg = accent?.DarkSidebarPrimaryForeground ?? S50;

        return $$"""

            :root {
              --background: oklch(1 0 0);
              --foreground: {{S950}};
              --card: oklch(1 0 0);
              --card-foreground: {{S950}};
              --popover: oklch(1 0 0);
              --popover-foreground: {{S950}};
              --primary: {{lPrimary}};
              --primary-foreground: {{lPrimaryFg}};
              --secondary: {{lSecondary}};
              --secondary-foreground: {{lSecondaryFg}};
              --muted: {{S100}};
              --muted-foreground: {{S500}};
              --accent: {{S100}};
              --accent-foreground: {{S900}};
              --destructive: oklch(0.577 0.245 27.325);
              --border: {{S200}};
              --input: {{S200}};
              --ring: {{S400}};
              --chart-1: {{lChart1}};
              --chart-2: {{lChart2}};
              --chart-3: {{lChart3}};
              --chart-4: {{lChart4}};
              --chart-5: {{lChart5}};
              --radius: 0.625rem;
              --sidebar: {{S50}};
              --sidebar-foreground: {{S950}};
              --sidebar-primary: {{lSidebarPrimary}};
              --sidebar-primary-foreground: {{lSidebarPrimaryFg}};
              --sidebar-accent: {{S100}};
              --sidebar-accent-foreground: {{S900}};
              --sidebar-border: {{S200}};
              --sidebar-ring: {{S400}};
            }

            .dark {
              --background: {{S950}};
              --foreground: {{S50}};
              --card: {{S900}};
              --card-foreground: {{S50}};
              --popover: {{S900}};
              --popover-foreground: {{S50}};
              --primary: {{dPrimary}};
              --primary-foreground: {{dPrimaryFg}};
              --secondary: {{dSecondary}};
              --secondary-foreground: {{dSecondaryFg}};
              --muted: {{S800}};
              --muted-foreground: {{S400}};
              --accent: {{S800}};
              --accent-foreground: {{S50}};
              --destructive: oklch(0.704 0.191 22.216);
              --border: oklch(1 0 0 / 10%);
              --input: oklch(1 0 0 / 15%);
              --ring: {{S500}};
              --chart-1: {{dChart1}};
              --chart-2: {{dChart2}};
              --chart-3: {{dChart3}};
              --chart-4: {{dChart4}};
              --chart-5: {{dChart5}};
              --sidebar: {{S900}};
              --sidebar-foreground: {{S50}};
              --sidebar-primary: {{dSidebarPrimary}};
              --sidebar-primary-foreground: {{dSidebarPrimaryFg}};
              --sidebar-accent: {{S800}};
              --sidebar-accent-foreground: {{S50}};
              --sidebar-border: oklch(1 0 0 / 10%);
              --sidebar-ring: {{S500}};
            }
            """;
    }
}

/// <summary>
/// Chromatic accent color that overlays primary, chart, and sidebar-primary variables
/// on top of any <see cref="BaseColorScale"/>. Matches shadcn/ui v4's theme system where
/// base color (gray scale) and accent color are independent choices.
/// </summary>
internal sealed record AccentColor(
    string Name,
    // Light overrides
    string LightPrimary, string LightPrimaryForeground,
    string LightSecondary, string LightSecondaryForeground,
    string LightChart1, string LightChart2, string LightChart3, string LightChart4, string LightChart5,
    string LightSidebarPrimary, string LightSidebarPrimaryForeground,
    // Dark overrides
    string DarkPrimary, string DarkPrimaryForeground,
    string DarkSecondary, string DarkSecondaryForeground,
    string DarkChart1, string DarkChart2, string DarkChart3, string DarkChart4, string DarkChart5,
    string DarkSidebarPrimary, string DarkSidebarPrimaryForeground)
{
    internal static readonly AccentColor Amber = new(
        "amber",
        // Light
        "oklch(0.555 0.163 48.998)", "oklch(0.987 0.022 95.277)",
        "oklch(0.967 0.001 286.375)", "oklch(0.21 0.006 285.885)",
        "oklch(0.879 0.169 91.605)", "oklch(0.769 0.188 70.08)",
        "oklch(0.666 0.179 58.318)", "oklch(0.555 0.163 48.998)", "oklch(0.473 0.137 46.201)",
        "oklch(0.666 0.179 58.318)", "oklch(0.987 0.022 95.277)",
        // Dark
        "oklch(0.473 0.137 46.201)", "oklch(0.987 0.022 95.277)",
        "oklch(0.274 0.006 286.033)", "oklch(0.985 0 0)",
        "oklch(0.879 0.169 91.605)", "oklch(0.769 0.188 70.08)",
        "oklch(0.666 0.179 58.318)", "oklch(0.555 0.163 48.998)", "oklch(0.473 0.137 46.201)",
        "oklch(0.769 0.188 70.08)", "oklch(0.279 0.077 45.635)");

    internal static readonly AccentColor Blue = new(
        "blue",
        // Light
        "oklch(0.488 0.243 264.376)", "oklch(0.97 0.014 254.604)",
        "oklch(0.967 0.001 286.375)", "oklch(0.21 0.006 285.885)",
        "oklch(0.809 0.105 251.813)", "oklch(0.623 0.214 259.815)",
        "oklch(0.546 0.245 262.881)", "oklch(0.488 0.243 264.376)", "oklch(0.424 0.199 265.638)",
        "oklch(0.546 0.245 262.881)", "oklch(0.97 0.014 254.604)",
        // Dark
        "oklch(0.424 0.199 265.638)", "oklch(0.97 0.014 254.604)",
        "oklch(0.274 0.006 286.033)", "oklch(0.985 0 0)",
        "oklch(0.809 0.105 251.813)", "oklch(0.623 0.214 259.815)",
        "oklch(0.546 0.245 262.881)", "oklch(0.488 0.243 264.376)", "oklch(0.424 0.199 265.638)",
        "oklch(0.623 0.214 259.815)", "oklch(0.97 0.014 254.604)");

    internal static readonly AccentColor Cyan = new(
        "cyan",
        // Light
        "oklch(0.52 0.105 223.128)", "oklch(0.984 0.019 200.873)",
        "oklch(0.967 0.001 286.375)", "oklch(0.21 0.006 285.885)",
        "oklch(0.865 0.127 207.078)", "oklch(0.715 0.143 215.221)",
        "oklch(0.609 0.126 221.723)", "oklch(0.52 0.105 223.128)", "oklch(0.45 0.085 224.283)",
        "oklch(0.609 0.126 221.723)", "oklch(0.984 0.019 200.873)",
        // Dark
        "oklch(0.45 0.085 224.283)", "oklch(0.984 0.019 200.873)",
        "oklch(0.274 0.006 286.033)", "oklch(0.985 0 0)",
        "oklch(0.865 0.127 207.078)", "oklch(0.715 0.143 215.221)",
        "oklch(0.609 0.126 221.723)", "oklch(0.52 0.105 223.128)", "oklch(0.45 0.085 224.283)",
        "oklch(0.715 0.143 215.221)", "oklch(0.302 0.056 229.695)");

    internal static readonly AccentColor Emerald = new(
        "emerald",
        // Light
        "oklch(0.508 0.118 165.612)", "oklch(0.979 0.021 166.113)",
        "oklch(0.967 0.001 286.375)", "oklch(0.21 0.006 285.885)",
        "oklch(0.845 0.143 164.978)", "oklch(0.696 0.17 162.48)",
        "oklch(0.596 0.145 163.225)", "oklch(0.508 0.118 165.612)", "oklch(0.432 0.095 166.913)",
        "oklch(0.596 0.145 163.225)", "oklch(0.979 0.021 166.113)",
        // Dark
        "oklch(0.432 0.095 166.913)", "oklch(0.979 0.021 166.113)",
        "oklch(0.274 0.006 286.033)", "oklch(0.985 0 0)",
        "oklch(0.845 0.143 164.978)", "oklch(0.696 0.17 162.48)",
        "oklch(0.596 0.145 163.225)", "oklch(0.508 0.118 165.612)", "oklch(0.432 0.095 166.913)",
        "oklch(0.696 0.17 162.48)", "oklch(0.262 0.051 172.552)");

    internal static readonly AccentColor Fuchsia = new(
        "fuchsia",
        // Light
        "oklch(0.518 0.253 323.949)", "oklch(0.977 0.017 320.058)",
        "oklch(0.967 0.001 286.375)", "oklch(0.21 0.006 285.885)",
        "oklch(0.833 0.145 321.434)", "oklch(0.667 0.295 322.15)",
        "oklch(0.591 0.293 322.896)", "oklch(0.518 0.253 323.949)", "oklch(0.452 0.211 324.591)",
        "oklch(0.591 0.293 322.896)", "oklch(0.977 0.017 320.058)",
        // Dark
        "oklch(0.452 0.211 324.591)", "oklch(0.977 0.017 320.058)",
        "oklch(0.274 0.006 286.033)", "oklch(0.985 0 0)",
        "oklch(0.833 0.145 321.434)", "oklch(0.667 0.295 322.15)",
        "oklch(0.591 0.293 322.896)", "oklch(0.518 0.253 323.949)", "oklch(0.452 0.211 324.591)",
        "oklch(0.667 0.295 322.15)", "oklch(0.977 0.017 320.058)");

    internal static readonly AccentColor Green = new(
        "green",
        // Light
        "oklch(0.532 0.157 131.589)", "oklch(0.986 0.031 120.757)",
        "oklch(0.967 0.001 286.375)", "oklch(0.21 0.006 285.885)",
        "oklch(0.871 0.15 154.449)", "oklch(0.723 0.219 149.579)",
        "oklch(0.627 0.194 149.214)", "oklch(0.527 0.154 150.069)", "oklch(0.448 0.119 151.328)",
        "oklch(0.648 0.2 131.684)", "oklch(0.986 0.031 120.757)",
        // Dark
        "oklch(0.453 0.124 130.933)", "oklch(0.986 0.031 120.757)",
        "oklch(0.274 0.006 286.033)", "oklch(0.985 0 0)",
        "oklch(0.871 0.15 154.449)", "oklch(0.723 0.219 149.579)",
        "oklch(0.627 0.194 149.214)", "oklch(0.527 0.154 150.069)", "oklch(0.448 0.119 151.328)",
        "oklch(0.768 0.233 130.85)", "oklch(0.986 0.031 120.757)");

    internal static readonly AccentColor Indigo = new(
        "indigo",
        // Light
        "oklch(0.457 0.24 277.023)", "oklch(0.962 0.018 272.314)",
        "oklch(0.967 0.001 286.375)", "oklch(0.21 0.006 285.885)",
        "oklch(0.785 0.115 274.713)", "oklch(0.585 0.233 277.117)",
        "oklch(0.511 0.262 276.966)", "oklch(0.457 0.24 277.023)", "oklch(0.398 0.195 277.366)",
        "oklch(0.511 0.262 276.966)", "oklch(0.962 0.018 272.314)",
        // Dark
        "oklch(0.398 0.195 277.366)", "oklch(0.962 0.018 272.314)",
        "oklch(0.274 0.006 286.033)", "oklch(0.985 0 0)",
        "oklch(0.785 0.115 274.713)", "oklch(0.585 0.233 277.117)",
        "oklch(0.511 0.262 276.966)", "oklch(0.457 0.24 277.023)", "oklch(0.398 0.195 277.366)",
        "oklch(0.585 0.233 277.117)", "oklch(0.962 0.018 272.314)");

    internal static readonly AccentColor Lime = new(
        "lime",
        // Light
        "oklch(0.532 0.157 131.589)", "oklch(0.986 0.031 120.757)",
        "oklch(0.967 0.001 286.375)", "oklch(0.21 0.006 285.885)",
        "oklch(0.897 0.196 126.665)", "oklch(0.768 0.233 130.85)",
        "oklch(0.648 0.2 131.684)", "oklch(0.532 0.157 131.589)", "oklch(0.453 0.124 130.933)",
        "oklch(0.648 0.2 131.684)", "oklch(0.986 0.031 120.757)",
        // Dark
        "oklch(0.453 0.124 130.933)", "oklch(0.986 0.031 120.757)",
        "oklch(0.274 0.006 286.033)", "oklch(0.985 0 0)",
        "oklch(0.897 0.196 126.665)", "oklch(0.768 0.233 130.85)",
        "oklch(0.648 0.2 131.684)", "oklch(0.532 0.157 131.589)", "oklch(0.453 0.124 130.933)",
        "oklch(0.768 0.233 130.85)", "oklch(0.274 0.072 132.109)");

    internal static readonly AccentColor Orange = new(
        "orange",
        // Light
        "oklch(0.553 0.195 38.402)", "oklch(0.98 0.016 73.684)",
        "oklch(0.967 0.001 286.375)", "oklch(0.21 0.006 285.885)",
        "oklch(0.837 0.128 66.29)", "oklch(0.705 0.213 47.604)",
        "oklch(0.646 0.222 41.116)", "oklch(0.553 0.195 38.402)", "oklch(0.47 0.157 37.304)",
        "oklch(0.646 0.222 41.116)", "oklch(0.98 0.016 73.684)",
        // Dark
        "oklch(0.47 0.157 37.304)", "oklch(0.98 0.016 73.684)",
        "oklch(0.274 0.006 286.033)", "oklch(0.985 0 0)",
        "oklch(0.837 0.128 66.29)", "oklch(0.705 0.213 47.604)",
        "oklch(0.646 0.222 41.116)", "oklch(0.553 0.195 38.402)", "oklch(0.47 0.157 37.304)",
        "oklch(0.705 0.213 47.604)", "oklch(0.98 0.016 73.684)");

    internal static readonly AccentColor Pink = new(
        "pink",
        // Light
        "oklch(0.525 0.223 3.958)", "oklch(0.971 0.014 343.198)",
        "oklch(0.967 0.001 286.375)", "oklch(0.21 0.006 285.885)",
        "oklch(0.823 0.12 346.018)", "oklch(0.656 0.241 354.308)",
        "oklch(0.592 0.249 0.584)", "oklch(0.525 0.223 3.958)", "oklch(0.459 0.187 3.815)",
        "oklch(0.592 0.249 0.584)", "oklch(0.971 0.014 343.198)",
        // Dark
        "oklch(0.459 0.187 3.815)", "oklch(0.971 0.014 343.198)",
        "oklch(0.274 0.006 286.033)", "oklch(0.985 0 0)",
        "oklch(0.823 0.12 346.018)", "oklch(0.656 0.241 354.308)",
        "oklch(0.592 0.249 0.584)", "oklch(0.525 0.223 3.958)", "oklch(0.459 0.187 3.815)",
        "oklch(0.656 0.241 354.308)", "oklch(0.971 0.014 343.198)");

    internal static readonly AccentColor Purple = new(
        "purple",
        // Light
        "oklch(0.496 0.265 301.924)", "oklch(0.977 0.014 308.299)",
        "oklch(0.967 0.001 286.375)", "oklch(0.21 0.006 285.885)",
        "oklch(0.827 0.119 306.383)", "oklch(0.627 0.265 303.9)",
        "oklch(0.558 0.288 302.321)", "oklch(0.496 0.265 301.924)", "oklch(0.438 0.218 303.724)",
        "oklch(0.558 0.288 302.321)", "oklch(0.977 0.014 308.299)",
        // Dark
        "oklch(0.438 0.218 303.724)", "oklch(0.977 0.014 308.299)",
        "oklch(0.274 0.006 286.033)", "oklch(0.985 0 0)",
        "oklch(0.827 0.119 306.383)", "oklch(0.627 0.265 303.9)",
        "oklch(0.558 0.288 302.321)", "oklch(0.496 0.265 301.924)", "oklch(0.438 0.218 303.724)",
        "oklch(0.627 0.265 303.9)", "oklch(0.977 0.014 308.299)");

    internal static readonly AccentColor Red = new(
        "red",
        // Light
        "oklch(0.505 0.213 27.518)", "oklch(0.971 0.013 17.38)",
        "oklch(0.967 0.001 286.375)", "oklch(0.21 0.006 285.885)",
        "oklch(0.808 0.114 19.571)", "oklch(0.637 0.237 25.331)",
        "oklch(0.577 0.245 27.325)", "oklch(0.505 0.213 27.518)", "oklch(0.444 0.177 26.899)",
        "oklch(0.577 0.245 27.325)", "oklch(0.971 0.013 17.38)",
        // Dark
        "oklch(0.444 0.177 26.899)", "oklch(0.971 0.013 17.38)",
        "oklch(0.274 0.006 286.033)", "oklch(0.985 0 0)",
        "oklch(0.808 0.114 19.571)", "oklch(0.637 0.237 25.331)",
        "oklch(0.577 0.245 27.325)", "oklch(0.505 0.213 27.518)", "oklch(0.444 0.177 26.899)",
        "oklch(0.637 0.237 25.331)", "oklch(0.971 0.013 17.38)");

    internal static readonly AccentColor Rose = new(
        "rose",
        // Light
        "oklch(0.514 0.222 16.935)", "oklch(0.969 0.015 12.422)",
        "oklch(0.967 0.001 286.375)", "oklch(0.21 0.006 285.885)",
        "oklch(0.81 0.117 11.638)", "oklch(0.645 0.246 16.439)",
        "oklch(0.586 0.253 17.585)", "oklch(0.514 0.222 16.935)", "oklch(0.455 0.188 13.697)",
        "oklch(0.586 0.253 17.585)", "oklch(0.969 0.015 12.422)",
        // Dark
        "oklch(0.455 0.188 13.697)", "oklch(0.969 0.015 12.422)",
        "oklch(0.274 0.006 286.033)", "oklch(0.985 0 0)",
        "oklch(0.81 0.117 11.638)", "oklch(0.645 0.246 16.439)",
        "oklch(0.586 0.253 17.585)", "oklch(0.514 0.222 16.935)", "oklch(0.455 0.188 13.697)",
        "oklch(0.645 0.246 16.439)", "oklch(0.969 0.015 12.422)");

    internal static readonly AccentColor Sky = new(
        "sky",
        // Light
        "oklch(0.5 0.134 242.749)", "oklch(0.977 0.013 236.62)",
        "oklch(0.967 0.001 286.375)", "oklch(0.21 0.006 285.885)",
        "oklch(0.828 0.111 230.318)", "oklch(0.685 0.169 237.323)",
        "oklch(0.588 0.158 241.966)", "oklch(0.5 0.134 242.749)", "oklch(0.443 0.11 240.79)",
        "oklch(0.588 0.158 241.966)", "oklch(0.977 0.013 236.62)",
        // Dark
        "oklch(0.443 0.11 240.79)", "oklch(0.977 0.013 236.62)",
        "oklch(0.274 0.006 286.033)", "oklch(0.985 0 0)",
        "oklch(0.828 0.111 230.318)", "oklch(0.685 0.169 237.323)",
        "oklch(0.588 0.158 241.966)", "oklch(0.5 0.134 242.749)", "oklch(0.443 0.11 240.79)",
        "oklch(0.685 0.169 237.323)", "oklch(0.293 0.066 243.157)");

    internal static readonly AccentColor Teal = new(
        "teal",
        // Light
        "oklch(0.511 0.096 186.391)", "oklch(0.984 0.014 180.72)",
        "oklch(0.967 0.001 286.375)", "oklch(0.21 0.006 285.885)",
        "oklch(0.855 0.138 181.071)", "oklch(0.704 0.14 182.503)",
        "oklch(0.6 0.118 184.704)", "oklch(0.511 0.096 186.391)", "oklch(0.437 0.078 188.216)",
        "oklch(0.6 0.118 184.704)", "oklch(0.984 0.014 180.72)",
        // Dark
        "oklch(0.437 0.078 188.216)", "oklch(0.984 0.014 180.72)",
        "oklch(0.274 0.006 286.033)", "oklch(0.985 0 0)",
        "oklch(0.855 0.138 181.071)", "oklch(0.704 0.14 182.503)",
        "oklch(0.6 0.118 184.704)", "oklch(0.511 0.096 186.391)", "oklch(0.437 0.078 188.216)",
        "oklch(0.704 0.14 182.503)", "oklch(0.277 0.046 192.524)");

    internal static readonly AccentColor Violet = new(
        "violet",
        // Light
        "oklch(0.491 0.27 292.581)", "oklch(0.969 0.016 293.756)",
        "oklch(0.967 0.001 286.375)", "oklch(0.21 0.006 285.885)",
        "oklch(0.811 0.111 293.571)", "oklch(0.606 0.25 292.717)",
        "oklch(0.541 0.281 293.009)", "oklch(0.491 0.27 292.581)", "oklch(0.432 0.232 292.759)",
        "oklch(0.541 0.281 293.009)", "oklch(0.969 0.016 293.756)",
        // Dark
        "oklch(0.432 0.232 292.759)", "oklch(0.969 0.016 293.756)",
        "oklch(0.274 0.006 286.033)", "oklch(0.985 0 0)",
        "oklch(0.811 0.111 293.571)", "oklch(0.606 0.25 292.717)",
        "oklch(0.541 0.281 293.009)", "oklch(0.491 0.27 292.581)", "oklch(0.432 0.232 292.759)",
        "oklch(0.606 0.25 292.717)", "oklch(0.969 0.016 293.756)");

    internal static readonly AccentColor Yellow = new(
        "yellow",
        // Light
        "oklch(0.852 0.199 91.936)", "oklch(0.421 0.095 57.708)",
        "oklch(0.967 0.001 286.375)", "oklch(0.21 0.006 285.885)",
        "oklch(0.905 0.182 98.111)", "oklch(0.795 0.184 86.047)",
        "oklch(0.681 0.162 75.834)", "oklch(0.554 0.135 66.442)", "oklch(0.476 0.114 61.907)",
        "oklch(0.681 0.162 75.834)", "oklch(0.987 0.026 102.212)",
        // Dark
        "oklch(0.795 0.184 86.047)", "oklch(0.421 0.095 57.708)",
        "oklch(0.274 0.006 286.033)", "oklch(0.985 0 0)",
        "oklch(0.905 0.182 98.111)", "oklch(0.795 0.184 86.047)",
        "oklch(0.681 0.162 75.834)", "oklch(0.554 0.135 66.442)", "oklch(0.476 0.114 61.907)",
        "oklch(0.795 0.184 86.047)", "oklch(0.987 0.026 102.212)");

    internal static IReadOnlyList<AccentColor> All { get; } =
        [Amber, Blue, Cyan, Emerald, Fuchsia, Green, Indigo, Lime, Orange, Pink, Purple, Red, Rose, Sky, Teal, Violet, Yellow];

    internal static AccentColor? Find(string name) =>
        All.FirstOrDefault(a => a.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
}
