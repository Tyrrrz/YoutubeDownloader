using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;

namespace YoutubeDownloader.Converters;

public class BoldMarkup
{
    private BoldMarkup() { }

    public static readonly AttachedProperty<string?> TextProperty =
        AvaloniaProperty.RegisterAttached<BoldMarkup, TextBlock, string?>("Text");

    static BoldMarkup()
    {
        TextProperty.Changed.AddClassHandler<TextBlock>(OnTextChanged);
    }

    public static string? GetText(TextBlock element) => element.GetValue(TextProperty);

    public static void SetText(TextBlock element, string? value) =>
        element.SetValue(TextProperty, value);

    private static void OnTextChanged(TextBlock textBlock, AvaloniaPropertyChangedEventArgs e)
    {
        textBlock.Inlines ??= [];
        textBlock.Inlines.Clear();

        if (e.NewValue is not string { Length: > 0 } text)
            return;

        var parts = text.Split("**");
        for (var i = 0; i < parts.Length; i++)
        {
            if (parts[i].Length == 0)
                continue;

            if (i % 2 == 0)
            {
                var lines = parts[i].Split('\n');
                for (var j = 0; j < lines.Length; j++)
                {
                    if (lines[j].Length > 0)
                        textBlock.Inlines.Add(new Run(lines[j]));
                    if (j < lines.Length - 1)
                        textBlock.Inlines.Add(new LineBreak());
                }
            }
            else
            {
                textBlock.Inlines.Add(new Run(parts[i]) { FontWeight = FontWeight.SemiBold });
            }
        }
    }
}
