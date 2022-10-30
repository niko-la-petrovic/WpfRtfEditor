using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
// TODO autofocus

/// <summary>
/// Interaction logic for FindWindow.xaml
/// </summary>
public partial class FindWindow : Window
{
    private readonly RichTextBox richTextBox;

    protected static string LastSearch { get; set; } = string.Empty;
    protected static int? NextOffset { get; set; } = null;

    public static FindWindowProperties Properties { get; set; } = new();

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
        var stringComparison = StringComparison.InvariantCultureIgnoreCase;
        if (matchCaseCheckbox.IsChecked == true)
            stringComparison = StringComparison.Ordinal;

        var foundTextRange = FindTextInRange(textRange, searchText, stringComparison);
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
        if (NextOffset != null && LastSearch == searchText)
            searchRange = new TextRange(searchRange.Start.GetPositionAtOffset(NextOffset.Value), searchRange.End);

        if (!Properties.LastSearches.Contains(searchText))
            Properties.AddLastSearch(searchText);
        LastSearch = searchText;

        var offset = searchRange.Text.IndexOf(searchText, stringComparison);
        if (offset < 0)
        {
            NextOffset = null;
            return null;  // Not found
        }
        NextOffset ??= 0;
        NextOffset += offset + searchText.Length;

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

    private void Previous_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (Previous.SelectedItem is null)
            return;

        findTextBox.Text = (string)Previous.SelectedItem;
        Previous.SelectedItem = null;
        e.Handled = true;
    }
}
