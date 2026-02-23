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

    private static readonly MarkdownPipeline MarkdownPipeline = new MarkdownPipelineBuilder()
        .UseEmphasisExtras()
        .Build();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var inlines = new InlineCollection();
        if (value is not string { Length: > 0 } text)
            return inlines;

        var document = Markdown.Parse(text, MarkdownPipeline);
        foreach (var block in document)
        {
            if (block is ParagraphBlock para && para.Inline is not null)
            {
                foreach (var markdownInline in para.Inline)
                    AddInlines(inlines, markdownInline);
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

    // Nullable FontWeight/FontStyle so that non-styled runs inherit properties
    // (e.g. FontWeight="Light") from the parent TextBlock rather than being
    // overridden with FontWeight.Normal, which would make space characters appear
    // heavier and visually wider than the surrounding text.
    private static void AddInlines(
        InlineCollection inlines,
        MarkdownInline markdownInline,
        FontWeight? fontWeight = null,
        FontStyle? fontStyle = null,
        TextDecorationCollection? textDecorations = null
    )
    {
        switch (markdownInline)
        {
            case LiteralInline literal:
                var run = new Run(literal.Content.ToString());
                if (fontWeight is not null)
                    run.FontWeight = fontWeight.Value;
                if (fontStyle is not null)
                    run.FontStyle = fontStyle.Value;
                if (textDecorations is not null)
                    run.TextDecorations = textDecorations;
                inlines.Add(run);
                break;

            case LineBreakInline:
                inlines.Add(new LineBreak());
                break;

            case EmphasisInline emphasis:
                var newWeight = fontWeight;
                var newStyle = fontStyle;
                var newDecorations = textDecorations;

                if (emphasis.DelimiterChar is '*' or '_' && emphasis.DelimiterCount >= 2)
                    newWeight = FontWeight.SemiBold;
                else if (emphasis.DelimiterChar is '*' or '_')
                    newStyle = FontStyle.Italic;
                else if (emphasis.DelimiterChar == '~')
                    newDecorations = TextDecorations.Strikethrough;
                else if (emphasis.DelimiterChar == '+')
                    newDecorations = TextDecorations.Underline;

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
