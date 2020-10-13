using DiscordChatExporter.Domain.Internal.Extensions;
using MaterialDesignThemes.Wpf;
using System;
using System.Linq;
using System.Windows.Media;

namespace YoutubeDownloader
{
    public sealed class Theme
    {
        public static Theme Light { get; } = MakeLightTheme(HexToColor.Convert("#343838"), HexToColor.Convert("#F9A825"));
        public static Theme Dark { get; } = MakeDarkTheme(HexToColor.Convert("#E8E8E8"), HexToColor.Convert("#F9A825"));

        public static Theme MakeLightTheme(Color primaryColor, Color secondaryColor)
        {
            return new Theme(
                new MaterialDesignLightTheme(),
                primaryColor,
                secondaryColor,
                Colors.DarkGreen,
                Colors.DarkOrange,
                Colors.DarkRed);
        }

        public static Theme MakeDarkTheme(Color primaryColor, Color secondaryColor)
        {
            return new Theme(
                new MaterialDesignDarkTheme(),
                primaryColor,
                secondaryColor,
                Colors.LightGreen,
                Colors.Orange,
                Colors.Red);
        }

        public static void SetCurrent(Theme theme)
        {
            SetMdixTheme();
            SetCustomColors();

            void SetMdixTheme()
            {
                var paletteHelper = new PaletteHelper();
                var materialTheme = paletteHelper.GetTheme();
                materialTheme.SetBaseTheme(theme.BaseTheme);
                materialTheme.SetPrimaryColor(theme.PrimaryColor);
                materialTheme.SetSecondaryColor(theme.SecondaryColor);
                paletteHelper.SetTheme(materialTheme);
            }

            void SetCustomColors()
            {
                var app = (App)System.Windows.Application.Current;
                string[] brushNames = new[] { "SuccessBrush", "CanceledBrush", "FailedBrush" };
                Color[] brushColors = new[] { theme.SuccessColor, theme.CanceledColor, theme.FailedColor };
                brushNames.Zip(brushColors, (brushName, color) => (brushName, color)).ForEach(tuple =>
                {
                    SolidColorBrush brush = (SolidColorBrush)app.TryFindResource(tuple.brushName);
                    if (brush == null)
                        throw new Exception($"Could not find brush with name \"{tuple.brushName}\".");
                    app.Resources[tuple.brushName] = new SolidColorBrush(tuple.color);
                });
            }
        }

        public Theme(IBaseTheme baseTheme, Color primaryColor, Color secondaryColor, Color successColor, Color canceledColor, Color failedColor)
        {
            BaseTheme = baseTheme;
            PrimaryColor = primaryColor;
            SecondaryColor = secondaryColor;
            SuccessColor = successColor;
            CanceledColor = canceledColor;
            FailedColor = failedColor;
        }

        public IBaseTheme BaseTheme { get; }
        public Color PrimaryColor { get; }
        public Color SecondaryColor { get; }

        public Color SuccessColor { get; }

        public Color CanceledColor { get; }

        public Color FailedColor { get; }

        class HexToColor
        {
            public static Color Convert(string hex)
            {
                return (Color)ColorConverter.ConvertFromString(hex);
            }
        }
    }
}
