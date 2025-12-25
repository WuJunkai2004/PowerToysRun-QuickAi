using System;
using System.Windows;
using System.Windows.Documents;

namespace Community.PowerToys.Run.Plugin.QuickAI
{
    public partial class ResultsWindow : Window
    {
        public ResultsWindow()
        {
            InitializeComponent();
        }

        public void AppendText(string text)
        {
            // keep UI thread safety
            Dispatcher.Invoke(() =>
            {
                // now use a sampler way to append text
                // just add a new Run to the Paragraph Inlines
                // the markdown rendering will handle the rest
                MainParagraph.Inlines.Add(new Run(text));
                
                // scroll to end
                OutputViewer.ScrollToEnd();
            });
        }

        public void SetFullText(string text)
        {
            Dispatcher.Invoke(() =>
            {
                MainParagraph.Inlines.Clear();
                MainParagraph.Inlines.Add(new Run(text));
            });
        }
    }
}