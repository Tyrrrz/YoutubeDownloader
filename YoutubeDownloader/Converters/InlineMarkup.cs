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

    private static void ProcessInline(
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
            {
                var run = new Run(literal.Content.ToString());

                if (fontWeight is not null)
                    run.FontWeight = fontWeight.Value;
                if (fontStyle is not null)
                    run.FontStyle = fontStyle.Value;
                if (textDecorations is not null)
                    run.TextDecorations = textDecorations;

                inlines.Add(run);
                break;
            }

            case LineBreakInline:
            {
                inlines.Add(new LineBreak());
                break;
            }

            case EmphasisInline emphasis:
            {
                var newWeight = fontWeight;
                var newStyle = fontStyle;
                var newDecorations = textDecorations;

                switch (emphasis.DelimiterChar)
                {
                    case '*' or '_' when emphasis.DelimiterCount == 2:
                        newWeight = FontWeight.SemiBold;
                        break;
                    case '*' or '_':
                        newStyle = FontStyle.Italic;
                        break;
                    case '~':
                        newDecorations = TextDecorations.Strikethrough;
                        break;
                    case '+':
                        newDecorations = TextDecorations.Underline;
                        break;
                }

                foreach (var child in emphasis)
                    ProcessInline(inlines, child, newWeight, newStyle, newDecorations);

                break;
            }

            case ContainerInline container:
            {
                foreach (var child in container)
                    ProcessInline(inlines, child, fontWeight, fontStyle, textDecorations);

                break;
            }
        }
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var inlines = new InlineCollection();
        if (value is not string { Length: > 0 } text)
            return inlines;

        foreach (var block in Markdown.Parse(text, MarkdownPipeline))
        {
            if (block is not ParagraphBlock { Inline: not null } paragraph)
                continue;

            foreach (var markdownInline in paragraph.Inline)
                ProcessInline(inlines, markdownInline);
        }

        return inlines;
    }

    public object? ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    ) => throw new NotSupportedException();
}
