using ColorPicker;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace WpfRtfEditor;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    protected RtfSelectionState RtfSelectionState { get; set; }
    protected TextSelection Selection => rtfTextBox.Selection;
    protected bool UnsavedChanges { get; set; }
    protected string FileLocation { get; set; }
    protected bool IsFileLocationSet => !string.IsNullOrWhiteSpace(FileLocation);
    protected bool HasAnyText => !string.IsNullOrWhiteSpace(GetFullTextRange().Text);
    protected bool IsSelectionEmpty => Selection.IsEmpty;
    protected bool RtfTextBoxInitialized => rtfTextBox != null;
    private readonly string _baseTitle;

    public static MainWindowProperties Properties { get; set; } = new();
    // TODO container control for grid columns of home
    // TODO subscript and superscript not working
    // TODO add input gesture text/shortcuts for all the menu items
    // TODO close command shortcut
    // TODO tooltips

    public MainWindow()
    {
        InitializeComponent();
        _baseTitle = Title;

        var fonts = Fonts.SystemFontFamilies.OrderBy(f => f.Source);
        var fontSizes = new List<double>() { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 };
        RtfSelectionState = new RtfSelectionState
        {
            FontFamily = fonts.FirstOrDefault(f => f.Source == "Calibri"),
            FontSize = 11
        };

        fontComboBox.ItemsSource = fonts;
        fontComboBox.SelectedItem = RtfSelectionState.FontFamily;

        fontSizeComboBox.ItemsSource = fontSizes;
        fontSizeComboBox.SelectedItem = RtfSelectionState.FontSize;
    }

    private void FontComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count == 0) return;

        if (IsSelectionEmpty) return; // TODO

        Selection.ApplyPropertyValue(TextElement.FontFamilyProperty, fontComboBox.SelectedItem);
    }

    private void RtfTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!UnsavedChanges)
        {
            UnsavedChanges = true;
            SetTitle();
        }
    }

    private void FontSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count == 0) return;

        if (IsSelectionEmpty) return; //TODO

        double fontSize = (double)e.AddedItems[0];
        Selection.ApplyPropertyValue(TextElement.FontSizeProperty, fontSize);
        //rtfTextBox.FontSize = fontSize;
    }

    private void SelectionChanged(object sender, RoutedEventArgs e)
    {

    }

    private void CloseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        if (!UnsavedChanges)
        {
            Application.Current.Shutdown();
            return;
        }

        var result = MessageBox.Show("There are unsaved changes. Do you want to save changes?", "Close Attempt Warning", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
        if (result == MessageBoxResult.Cancel) return;
        else if (result == MessageBoxResult.No)
        {
            Application.Current.Shutdown();
            return;
        }
        else if (result == MessageBoxResult.Yes)
        {
            if (saveMenuItem.Command.CanExecute(null))
            {
                saveMenuItem.Command.Execute(null);
                Application.Current.Shutdown();
            }
        }
    }

    private void CloseCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = true;
    }

    private void SaveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(FileLocation))
        {
            SaveAsCommand_Executed(sender, e);
            return;
        }

        SaveContentsToFile(FileLocation);
    }

    private void SaveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = UnsavedChanges;
    }

    private void SaveAsCommand_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        var dialog = new SaveFileDialog
        {
            FileName = "Document",
            DefaultExt = ".rtf",
            Filter = ".rtf|*.rtf|.txt|*.txt"
        };

        bool? result = dialog.ShowDialog();
        if (result == true)
            SaveContentsToFile(dialog.FileName);
    }

    private void SaveContentsToFile(string fileName)
    {
        var extension = Path.GetExtension(fileName);
        var textDataFormat = GetTextDataFormatByExtension(extension);
        try
        {
            var textRange = GetFullTextRange();
            if (textDataFormat == TextDataFormat.Rtf)
            {
                using var fs = new FileStream(fileName, FileMode.Create);
                textRange.Save(fs, DataFormats.Rtf);
            }
            else if (textDataFormat == TextDataFormat.Text)
                File.WriteAllText(fileName, textRange.Text);
        }
        finally
        {
            FileLocation = fileName;
            UnsavedChanges = false;
            SetTitle();
        }
    }

    private static TextDataFormat GetTextDataFormatByExtension(string extension)
    {
        return extension == ".rtf" ? TextDataFormat.Rtf : TextDataFormat.Text;
    }

    private static string GetDataFormatByExtension(string extension)
    {
        return extension == ".rtf" ? DataFormats.Rtf : DataFormats.Text;
    }

    private void SetTitle()
    {
        Title = (UnsavedChanges ? "* - " : string.Empty) + _baseTitle + (IsFileLocationSet ? FileLocation : string.Empty);
    }

    private TextRange GetFullTextRange()
    {
        return new TextRange(rtfTextBox.Document.ContentStart, rtfTextBox.Document.ContentEnd);
    }

    private void SaveAsCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = true;
    }

    private void OpenCommand_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        if (UnsavedChanges)
            saveMenuItem.Command.Execute(null);

        var dialog = new OpenFileDialog
        {
            DefaultExt = ".rtf",
            Filter = "Text documents (.txt)|*.txt|(.rtf)|*.rtf"
        };
        var result = dialog.ShowDialog();
        if (result == true)
        {
            var fileName = dialog.FileName;
            var dataFormat = GetDataFormatByExtension(Path.GetExtension(fileName));
            var textRange = GetFullTextRange();
            using var fs = new FileStream(dialog.FileName, FileMode.Open);
            textRange.Load(fs, dataFormat);

            // TODO to method
            FileLocation = dialog.FileName;
            UnsavedChanges = false;
            SetTitle();
        }
    }

    private void OpenCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = true;
    }

    private void FindCommand_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        var findWindow = new FindWindow(rtfTextBox);
        findWindow.Show();
    }

    private void FindCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        if (!RtfTextBoxInitialized)
            e.CanExecute = false;
        else
            e.CanExecute = HasAnyText;
    }

    private void StrikethroughCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        SetExecutableIfInitialized(e);
    }

    private void SetExecutableIfInitialized(CanExecuteRoutedEventArgs e)
    {
        if (!RtfTextBoxInitialized)
            e.CanExecute = false;
        else
            e.CanExecute = true;

        e.Handled = true;
    }

    private void StrikethroughCommand_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        if (IsSelectionEmpty) return; // TODO

        var selectionDecorations = Selection.GetPropertyValue(Inline.TextDecorationsProperty) as TextDecorationCollection;
        if (selectionDecorations is null || !selectionDecorations.Any(td => td.Location == TextDecorationLocation.Strikethrough))
        {
            if (selectionDecorations is null)
                selectionDecorations ??= new TextDecorationCollection();
            if (selectionDecorations.IsFrozen)
                return;

            selectionDecorations.Add(TextDecorations.Strikethrough);
            Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, selectionDecorations); // TODO fix - for some reason - applying to the entire content - not just selection
            return;
        }
        else
        {
            var clearedDecorationsList = selectionDecorations
                .SkipWhile(sd => sd.Location == TextDecorationLocation.Strikethrough)
                .ToList();
            var clearedDecorations = new TextDecorationCollection(clearedDecorationsList);
            Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, clearedDecorations);
        }
    }

    private TextRange GetSelectedTextRange()
    {
        return new TextRange(Selection.Start, Selection.End);
    }

    private void ClearFormattingCommand_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        if (IsSelectionEmpty) return;

        var selectedTextRange = GetSelectedTextRange();
        selectedTextRange.ClearAllProperties();
    }

    private void ClearFormattingCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        SetExecutableIfInitialized(e);
    }

    private void BackgroundTextColorComboBox_Selected(object sender, SelectionChangedEventArgs e)
    {
        backgroundTextColorComboBox.SelectedItem = null;
        e.Handled = true;
    }

    private void ForegroundTextColorComboBox_Selected(object sender, SelectionChangedEventArgs e)
    {
        foregroundTextColorComboBox.SelectedItem = null;
        e.Handled = true;
    }

    private void ForegroundTextColorPicker_ColorChanged(object sender, RoutedEventArgs e)
    {
        if (IsSelectionEmpty) return; // TODO

        Properties.ForegroundTextBrush = new SolidColorBrush(
                FromColorPicker(foregroundTextColorPicker));

        Selection.ApplyPropertyValue(
            TextElement.ForegroundProperty,
            Properties.ForegroundTextBrush
            );
    }

    private void BackgroundTextColorPicker_ColorChanged(object sender, RoutedEventArgs e)
    {
        if (IsSelectionEmpty) return; // TODO

        Properties.BackgroundTextBrush = new SolidColorBrush(
                FromColorPicker(backgroundTextColorPicker
            ));

        Selection.ApplyPropertyValue(
            TextElement.BackgroundProperty,
            Properties.BackgroundTextBrush);
    }

    private static Color FromColorPicker(StandardColorPicker colorPicker)
    {
        return Color.FromArgb(
                    (byte)colorPicker.Color.A,
                    (byte)colorPicker.Color.RGB_R,
                    (byte)colorPicker.Color.RGB_G,
                    (byte)colorPicker.Color.RGB_B);
    }

    private void SelectAllCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        rtfTextBox.Focus();
        rtfTextBox.SelectAll();
        var range = GetSelectedTextRange();
    }

    private void SelectAllCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = true;
    }
}
