using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfRtfEditor;

// TODO label alt+_ for the find window
// TODO focus back onto window if found
// TODO find next should go to the next index if text unchanged

/// <summary>
/// Interaction logic for FindWindow.xaml
/// </summary>
public partial class FindWindow : Window
{
    private readonly RichTextBox richTextBox;

    protected string LastSearch { get; set; } = string.Empty;
    protected int? LastIndex { get; set; } = null;

    public FindWindow()
    {
        InitializeComponent();
    }

    public FindWindow(RichTextBox richTextBox)
    {
        InitializeComponent();
        this.richTextBox = richTextBox;
    }

    private void FindCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = !string.IsNullOrWhiteSpace(findTextBox.Text);
        e.Handled = true;
    }

    private void FindCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        var searchText = e.Parameter as string;
        if (string.IsNullOrWhiteSpace(searchText))
            return;

        var textRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
        var foundTextRange = FindTextInRange(textRange, searchText, StringComparison.Ordinal);
        if (foundTextRange is null)
        {
            MessageBox.Show("No results found.", "Search Results", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        richTextBox.Selection.Select(foundTextRange.Start, foundTextRange.End);
        richTextBox.Focus();
    }

    public static TextRange? FindTextInRange(TextRange searchRange, string searchText, StringComparison stringComparison)
    {
        // TODO last search string and index
        int offset = searchRange.Text.IndexOf(searchText, stringComparison);
        if (offset < 0)
            return null;  // Not found

        var start = GetTextPositionAtOffset(searchRange.Start, offset);
        var result = new TextRange(start, GetTextPositionAtOffset(start, searchText.Length));

        return result;
    }

    public static TextPointer? GetTextPositionAtOffset(TextPointer? position, int characterCount)
    {
        while (position != null)
        {
            if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
            {
                int count = position.GetTextRunLength(LogicalDirection.Forward);
                if (characterCount <= count)
                {
                    return position.GetPositionAtOffset(characterCount);
                }

                characterCount -= count;
            }

            TextPointer nextContextPosition = position.GetNextContextPosition(LogicalDirection.Forward);
            if (nextContextPosition == null)
                return position;

            position = nextContextPosition;
        }

        return position;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
