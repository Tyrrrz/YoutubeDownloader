using System;
using System.Globalization;
using Avalonia.Controls.Documents;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using MarkdownInline = Markdig.Syntax.Inlines.Inline;

namespace YoutubeDownloader.Converters;

public class InlineMarkup : IValueConverter
{
    public static readonly InlineMarkup Instance = new();

    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseEmphasisExtras()
        .Build();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string { Length: > 0 } text)
            return null;

        var inlines = new InlineCollection();
        var document = Markdown.Parse(text, Pipeline);
        foreach (var block in document)
        {
            if (block is ParagraphBlock para && para.Inline is not null)
            {
                foreach (var inline in para.Inline)
                    AddInlines(inlines, inline);
            }
        }

        return inlines;
    }

    public object? ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    ) => throw new NotSupportedException();

    private static void AddInlines(
        InlineCollection inlines,
        MarkdownInline inline,
        FontWeight fontWeight = FontWeight.Normal,
        FontStyle fontStyle = FontStyle.Normal,
        TextDecorationCollection? textDecorations = null
    )
    {
        switch (inline)
        {
            case LiteralInline literal:
                inlines.Add(
                    new Run(literal.Content.ToString())
                    {
                        FontWeight = fontWeight,
                        FontStyle = fontStyle,
                        TextDecorations = textDecorations,
                    }
                );
                break;

            case LineBreakInline:
                inlines.Add(new LineBreak());
                break;

            case EmphasisInline emphasis:
                var (newWeight, newStyle, newDecorations) = emphasis.DelimiterChar switch
                {
                    '*' or '_' when emphasis.DelimiterCount >= 2 => (
                        FontWeight.SemiBold,
                        fontStyle,
                        textDecorations
                    ),
                    '*' or '_' => (fontWeight, FontStyle.Italic, textDecorations),
                    '~' => (fontWeight, fontStyle, TextDecorations.Strikethrough),
                    '+' => (fontWeight, fontStyle, TextDecorations.Underline),
                    _ => (fontWeight, fontStyle, textDecorations),
                };
                foreach (var child in emphasis)
                    AddInlines(inlines, child, newWeight, newStyle, newDecorations);
                break;

            case ContainerInline container:
                foreach (var child in container)
                    AddInlines(inlines, child, fontWeight, fontStyle, textDecorations);
                break;
        }
    }
}
