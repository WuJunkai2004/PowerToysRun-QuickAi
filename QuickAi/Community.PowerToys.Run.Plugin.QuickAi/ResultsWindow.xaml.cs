using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Community.PowerToys.Run.Plugin.QuickAI
{
    public partial class ResultsWindow : Window
    {
        private MarkdownRenderer _markdownRenderer;
        private bool _isDarkTheme = true;

        public ResultsWindow()
        {
            InitializeComponent();
            InitializeRenderer();
        }

        private void InitializeRenderer()
        {
            _markdownRenderer = new MarkdownRenderer(MainParagraph, _isDarkTheme);
        }

        public void AppendText(string text)
        {
            // keep UI thread safety
            Dispatcher.BeginInvoke(() =>
            {
                // Use MarkdownRenderer for streaming rendering
                _markdownRenderer.Append(text);

                // scroll to end: bring last block into view
                try
                {
                    var last = OutputViewer.Document?.Blocks.LastBlock;
                    last?.BringIntoView();
                }
                catch
                {
                }
            });
        }

        public void SetFullText(string text)
        {
            Dispatcher.Invoke(() =>
            {
                MainParagraph.Inlines.Clear();
                InitializeRenderer();
                _markdownRenderer.Append(text);
                _markdownRenderer.Flush();
            });
        }

        /// <summary>
        /// Flush the renderer cache to ensure all content is output
        /// </summary>
        public void FlushRenderer()
        {
            Dispatcher.BeginInvoke(() =>
            {
                _markdownRenderer?.Flush();
            });
        }

        /// <summary>
        /// Clear content and reset the renderer
        /// </summary>
        public void Clear()
        {
            Dispatcher.Invoke(() =>
            {
                MainParagraph.Inlines.Clear();
                InitializeRenderer();
            });
        }

        // Apply theme by strict lowercase string: "dark" or "light"
        public void ApplyTheme(string theme)
        {
            if (theme != "dark" && theme != "light")
            {
                return;
            }

            Dispatcher.Invoke(() =>
            {
                try
                {
                    _isDarkTheme = theme == "dark";
                    
                    if (theme == "light")
                    {
                        Background = Brushes.White;
                        if (OutputViewer?.Document != null)
                        {
                            OutputViewer.Document.Foreground = Brushes.Black;
                            OutputViewer.Document.Background = Brushes.White;
                        }
                    }
                    else // "dark"
                    {
                        Background = new SolidColorBrush(Color.FromRgb(0x20, 0x20, 0x20));
                        if (OutputViewer?.Document != null)
                        {
                            OutputViewer.Document.Foreground = new SolidColorBrush(Color.FromRgb(0xE0, 0xE0, 0xE0));
                            OutputViewer.Document.Background = Brushes.Transparent;
                        }
                    }

                    // Re-initialize the renderer to use the new theme
                    // Note: This will not clear existing content, it only affects subsequent rendering
                    _markdownRenderer = new MarkdownRenderer(MainParagraph, _isDarkTheme);
                }
                catch
                {
                    // best-effort, swallow errors
                }
            });
        }
    }
}