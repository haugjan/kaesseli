using MudBlazor;

namespace Kaesseli.Client.Blazor.Theme;

public static class KaesseliTheme
{
    public static MudTheme Create() => new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#76B041",
            PrimaryContrastText = "#FFFFFF",
            Secondary = "#46BAB8",
            SecondaryContrastText = "#FFFFFF",
            Background = "#F0F2F5",
            Surface = "#FFFFFF",
            DrawerBackground = "#2B2D35",
            DrawerText = "#C8CDD8",
            DrawerIcon = "#8B9099",
            AppbarBackground = "#FFFFFF",
            AppbarText = "#2B2D35",
            TextPrimary = "#2B2D35",
            TextSecondary = "#6B7280",
            Success = "#76B041",
            Error = "#C10015",
            Warning = "#F2C037",
            Info = "#31CCEC",
            Divider = "#E5E7EB",
            ActionDefault = "#6B7280",
            TableLines = "#F0F2F5",
            TableStriped = "#F8F9FA",
            TableHover = "#F0F2F5",
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#8DC654",
            PrimaryContrastText = "#FFFFFF",
            Secondary = "#46BAB8",
            Background = "#1A1C22",
            Surface = "#22242C",
            DrawerBackground = "#15171C",
            DrawerText = "#C8CDD8",
            DrawerIcon = "#8B9099",
            AppbarBackground = "#22242C",
            AppbarText = "#F3F4F6",
            TextPrimary = "#F3F4F6",
            TextSecondary = "#9CA3AF",
            Success = "#8DC654",
            Error = "#EF4444",
            Warning = "#F2C037",
            Info = "#31CCEC",
            Divider = "#2F3139",
        },
        Typography = new Typography
        {
            Default = new DefaultTypography
            {
                FontFamily = ["Inter", "Roboto", "sans-serif"],
                FontSize = "0.875rem",
                FontWeight = "400",
                LineHeight = "1.5",
            },
            H1 = new H1Typography { FontSize = "2rem", FontWeight = "700" },
            H2 = new H2Typography { FontSize = "1.5rem", FontWeight = "600" },
            H3 = new H3Typography { FontSize = "1.25rem", FontWeight = "600" },
            H4 = new H4Typography { FontSize = "1.125rem", FontWeight = "600" },
            H5 = new H5Typography { FontSize = "1rem", FontWeight = "600" },
            H6 = new H6Typography { FontSize = "0.875rem", FontWeight = "600" },
            Button = new ButtonTypography { FontWeight = "500", TextTransform = "none" },
        },
        LayoutProperties = new LayoutProperties
        {
            DrawerWidthLeft = "240px",
            AppbarHeight = "56px",
        },
    };
}
