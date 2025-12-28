using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Community.PowerToys.Run.Plugin.QuickAI
{
    /// <summary>
    /// Markdown format enumeration
    /// </summary>
    public enum MarkdownFormat
    {
        Title,      // Title: Bold, Pink
        Bold,       // Bold: Bold, White
        Italic,     // Italic: Italic, White
        Code,       // Code: Yellow, Monospace
        List,       // List: Green
        Quote,      // Quote: Italic, Blue, Underline
        Common      // Common: White
    }

    /// <summary>
    /// Markdown streaming renderer
    /// Supports: Title(#), Bold(**), Italic(*/_), Inline Code(`), Code Block(```), List(-), Quote(>)
    /// </summary>
    public class MarkdownRenderer
    {
        private char _prev = '\0';                      // Record the previous character
        private readonly List<MarkdownFormat> _formats; // Record the current format stack
        private readonly List<string> _prefixes;        // Record the current string prefixes
        private readonly List<char> _strings;           // Record the current string content
        private readonly Paragraph _targetParagraph;    // Target paragraph
        private readonly bool _isDarkTheme;             // Whether it is a dark theme

        // Color definitions
        private static readonly SolidColorBrush TitleColor = new SolidColorBrush(Color.FromRgb(0xFF, 0x69, 0xB4)); // Pink
        private static readonly SolidColorBrush CodeColor = new SolidColorBrush(Color.FromRgb(0xFF, 0xD7, 0x00));  // Golden yellow
        private static readonly SolidColorBrush ListColor = new SolidColorBrush(Color.FromRgb(0x00, 0xC8, 0x53));  // Green
        private static readonly SolidColorBrush QuoteColor = new SolidColorBrush(Color.FromRgb(0x64, 0xB5, 0xF6)); // Blue
        private static readonly SolidColorBrush LightTextColor = new SolidColorBrush(Color.FromRgb(0x20, 0x20, 0x20));
        private static readonly SolidColorBrush DarkTextColor = new SolidColorBrush(Color.FromRgb(0xE0, 0xE0, 0xE0));
        private static readonly FontFamily CodeFontFamily = new FontFamily("Consolas, Courier New, monospace");

        static MarkdownRenderer()
        {
            // Freeze brushes to improve performance
            TitleColor.Freeze();
            CodeColor.Freeze();
            ListColor.Freeze();
            QuoteColor.Freeze();
            LightTextColor.Freeze();
            DarkTextColor.Freeze();
        }

        public MarkdownRenderer(Paragraph targetParagraph, bool isDarkTheme = true)
        {
            _targetParagraph = targetParagraph ?? throw new ArgumentNullException(nameof(targetParagraph));
            _isDarkTheme = isDarkTheme;
            _formats = new List<MarkdownFormat>();
            _prefixes = new List<string>();
            _strings = new List<char>();
        }

        /// <summary>
        /// Append text for rendering
        /// </summary>
        public void Append(string text)
        {
            if (string.IsNullOrEmpty(text)) return;

            foreach (char c in text)
            {
                ProcessChar(c);
                _prev = c;
            }
        }

        /// <summary>
        /// Flush the renderer to ensure all content is processed
        /// </summary>
        public void Flush()
        {
            FlushStrings();
        }

        private bool IsPrevEndLine() => _prev == '\n' || _prev == '\0';

        private bool IsInCodeBlock()
        {
            for (int i = 0; i < _formats.Count; i++)
            {
                if (_formats[i] == MarkdownFormat.Code && 
                    i < _prefixes.Count && 
                    _prefixes[i] == "```started")
                {
                    return true;
                }
            }
            return false;
        }

        private MarkdownFormat? GetLastFormat()
        {
            return _formats.Count > 0 ? _formats[^1] : null;
        }

        private string GetLastPrefix()
        {
            return _prefixes.Count > 0 ? _prefixes[^1] : null;
        }

        private void PopFormat()
        {
            if (_formats.Count > 0) _formats.RemoveAt(_formats.Count - 1);
            if (_prefixes.Count > 0) _prefixes.RemoveAt(_prefixes.Count - 1);
        }

        private void FlushStrings()
        {
            if (_strings.Count == 0) return;

            string content = new string(_strings.ToArray());
            _strings.Clear();

            var run = CreateStyledRun(content);
            _targetParagraph.Inlines.Add(run);
        }

        private Inline CreateStyledRun(string text)
        {
            var run = new System.Windows.Documents.Run(text);
            
            // Default color
            run.Foreground = _isDarkTheme ? DarkTextColor : LightTextColor;

            // Apply styles based on the format stack
            bool isBold = false;
            bool isItalic = false;
            bool isUnderline = false;
            Brush foreground = _isDarkTheme ? DarkTextColor : LightTextColor;
            FontFamily fontFamily = null;

            foreach (var fmt in _formats)
            {
                switch (fmt)
                {
                    case MarkdownFormat.Title:
                        isBold = true;
                        foreground = TitleColor;
                        break;
                    case MarkdownFormat.Bold:
                        isBold = true;
                        break;
                    case MarkdownFormat.Italic:
                        isItalic = true;
                        break;
                    case MarkdownFormat.Code:
                        foreground = CodeColor;
                        fontFamily = CodeFontFamily;
                        break;
                    case MarkdownFormat.List:
                        foreground = ListColor;
                        break;
                    case MarkdownFormat.Quote:
                        isItalic = true;
                        isUnderline = true;
                        foreground = QuoteColor;
                        break;
                }
            }

            run.Foreground = foreground;
            if (fontFamily != null) run.FontFamily = fontFamily;
            if (isBold) run.FontWeight = FontWeights.Bold;
            if (isItalic) run.FontStyle = FontStyles.Italic;
            
            if (isUnderline)
            {
                var underline = new Underline(run);
                return underline;
            }

            return run;
        }

        private void AppendLineBreak()
        {
            _targetParagraph.Inlines.Add(new LineBreak());
        }

        private void ProcessChar(char c)
        {
            // Handle multi-line code block content
            if (IsInCodeBlock())
            {
                if (c == '`')
                {
                    _strings.Add(c);
                    if (_strings.Count >= 3)
                    {
                        string last3 = new string(_strings.ToArray(), _strings.Count - 3, 3);
                        if (last3 == "```")
                        {
                            // Code block ends, remove the end marker and pop the format
                            _strings.RemoveRange(_strings.Count - 3, 3);
                            FlushStrings();
                            PopFormat();
                        }
                    }
                    return;
                }
                else if (c == '\n')
                {
                    FlushStrings();
                    AppendLineBreak();
                    return;
                }
                else
                {
                    _strings.Add(c);
                    return;
                }
            }

            // Handle the beginning of a line
            if (IsPrevEndLine())
            {
                if (c == '#')
                {
                    _formats.Add(MarkdownFormat.Title);
                    _prefixes.Add("#");
                    return;
                }
                else if (c == '-')
                {
                    _formats.Add(MarkdownFormat.List);
                    _prefixes.Add("-");
                    _strings.Add('•'); // Use a bullet point as a list marker
                    return;
                }
                else if (c == '>')
                {
                    _formats.Add(MarkdownFormat.Quote);
                    _prefixes.Add(">");
                    _strings.Add('>');
                    _strings.Add(' ');
                    return;
                }
                else if (c == '`')
                {
                    _prefixes.Add("`");
                    _formats.Add(MarkdownFormat.Code);
                    return;
                }
                else if (c == '\n' || c == '\r')
                {
                    // Empty line, clear the format stack
                    FlushStrings();
                    _formats.Clear();
                    _prefixes.Clear();
                    AppendLineBreak();
                    return;
                }
                else
                {
                    _strings.Add(c);
                    return;
                }
            }

            // Handle subsequent # in title lines
            if (GetLastFormat() == MarkdownFormat.Title && GetLastPrefix()?.StartsWith("#") == true)
            {
                if (c == '#')
                {
                    _prefixes[^1] += "#";
                    return;
                }
                else if (c == ' ')
                {
                    // Title prefix ends, start content
                    return;
                }
                else
                {
                    _strings.Add(c);
                    return;
                }
            }

            // Handle multi-line code block start ``` (only for cases starting at the beginning of a line)
            var lastPrefix = GetLastPrefix();
            if (lastPrefix != null && lastPrefix.StartsWith('`') && lastPrefix != "`inline")
            {
                if (!lastPrefix.StartsWith("```"))
                {
                    if (c == '`')
                    {
                        // Check if there is content, if so, it means the inline code ends
                        if (_strings.Count > 0)
                        {
                            FlushStrings();
                            PopFormat();
                            return;
                        }
                        // No content, continue accumulating backticks
                        _prefixes[^1] += "`";
                        return;
                    }
                    else if (lastPrefix == "``")
                    {
                        // Not three backticks, fallback processing
                        PopFormat();
                        _strings.Add('`');
                        _strings.Add('`');
                        _strings.Add(c);
                        return;
                    }
                    else
                    {
                        // Single backtick - inline code starts (at the beginning of a line)
                        if (c == '\n')
                        {
                            FlushStrings();
                            PopFormat();
                            AppendLineBreak();
                            return;
                        }
                        _strings.Add(c);
                        return;
                    }
                }
            }

            // Handle the language identifier of the multi-line code block (content after ``` until newline)
            if (lastPrefix == "```")
            {
                if (c == '\n')
                {
                    // Language identifier ends, start code content, modify marker
                    _prefixes[^1] = "```started";
                }
                return;
            }

            // Handle inline code (single backtick) - whether at the beginning or in the middle of a line
            if (GetLastFormat() == MarkdownFormat.Code && 
                (lastPrefix == "`" || lastPrefix == "`inline"))
            {
                if (c == '`')
                {
                    // Inline code ends
                    FlushStrings();
                    PopFormat();
                    return;
                }
                else if (c == '\n')
                {
                    FlushStrings();
                    PopFormat();
                    AppendLineBreak();
                    return;
                }
                else
                {
                    _strings.Add(c);
                    return;
                }
            }

            // Handle inline code start ` (not at the beginning of a line)
            if (c == '`')
            {
                FlushStrings();
                _formats.Add(MarkdownFormat.Code);
                _prefixes.Add("`inline");
                return;
            }

            // Handle bold **
            if (c == '*')
            {
                if (_prev == '*')
                {
                    // Check if it is the end of bold
                    if (GetLastFormat() == MarkdownFormat.Bold)
                    {
                        // Remove previously added *
                        if (_strings.Count > 0 && _strings[^1] == '*')
                        {
                            _strings.RemoveAt(_strings.Count - 1);
                        }
                        FlushStrings();
                        PopFormat();
                        return;
                    }
                    else
                    {
                        // Bold starts
                        if (_strings.Count > 0 && _strings[^1] == '*')
                        {
                            _strings.RemoveAt(_strings.Count - 1);
                        }
                        FlushStrings();
                        _formats.Add(MarkdownFormat.Bold);
                        _prefixes.Add("**");
                        return;
                    }
                }
                else
                {
                    _strings.Add(c);
                    return;
                }
            }

            // Handle italic * or _
            if ((c == '_' && _prev != '_') || (_prev == '*' && c != '*'))
            {
                // Check if the previous one is a single *
                if (_prev == '*' && _strings.Count > 0 && _strings[^1] == '*')
                {
                    // Single * italic
                    if (GetLastFormat() == MarkdownFormat.Italic && GetLastPrefix() == "*")
                    {
                        _strings.RemoveAt(_strings.Count - 1);
                        FlushStrings();
                        PopFormat();
                    }
                    else
                    {
                        _strings.RemoveAt(_strings.Count - 1);
                        FlushStrings();
                        _formats.Add(MarkdownFormat.Italic);
                        _prefixes.Add("*");
                    }
                    if (c != '*')
                    {
                        _strings.Add(c);
                    }
                    return;
                }
            }

            if (c == '_')
            {
                if (GetLastFormat() == MarkdownFormat.Italic && GetLastPrefix() == "_")
                {
                    FlushStrings();
                    PopFormat();
                    return;
                }
                else
                {
                    FlushStrings();
                    _formats.Add(MarkdownFormat.Italic);
                    _prefixes.Add("_");
                    return;
                }
            }

            // Handle newline
            if (c == '\n')
            {
                FlushStrings();
                // Clear line-level formats (title, list, quote)
                while (GetLastFormat() is MarkdownFormat fmt && 
                       (fmt == MarkdownFormat.Title || fmt == MarkdownFormat.List || fmt == MarkdownFormat.Quote))
                {
                    PopFormat();
                }
                AppendLineBreak();
                return;
            }

            // Common character
            _strings.Add(c);
        }
    }
}
