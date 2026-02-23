using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using MarkdownInline = Markdig.Syntax.Inlines.Inline;

namespace YoutubeDownloader.Converters;

public class InlineMarkup
{
    private InlineMarkup() { }

    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseEmphasisExtras()
        .Build();

    public static readonly AttachedProperty<string?> TextProperty =
        AvaloniaProperty.RegisterAttached<InlineMarkup, TextBlock, string?>("Text");

    static InlineMarkup()
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

        var document = Markdown.Parse(text, Pipeline);
        foreach (var block in document)
        {
            if (block is ParagraphBlock para && para.Inline is not null)
            {
                foreach (var inline in para.Inline)
                    AddInlines(textBlock.Inlines, inline);
            }
        }
    }

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
