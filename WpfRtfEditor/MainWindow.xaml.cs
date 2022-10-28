using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
    protected bool UnsavedChanges { get; set; } // TODO on save set to false
    protected string FileLocation { get; set; }
    protected bool IsFileLocationSet => !string.IsNullOrWhiteSpace(FileLocation);
    private readonly string _baseTitle;
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

        if (Selection.IsEmpty) return; // TODO


        Selection.ApplyPropertyValue(TextElement.FontFamilyProperty, fontComboBox.SelectedItem);
    }

    private void RtfTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!UnsavedChanges)
        {
            UnsavedChanges = true;
            SetTitle();
        }
        //Selection.Select(rtfTextBox.CaretPosition, rtfTextBox.CaretPosition.GetPositionAtOffset(-1));
        //// TODO only do this if unhandled property change
        //Selection.ApplyPropertyValue(TextElement.FontFamilyProperty, fontComboBox.SelectedItem);

        //Selection.Select(rtfTextBox.CaretPosition.DocumentEnd, rtfTextBox.CaretPosition.DocumentEnd);


        //Selection.Select()
        //
    }

    // TODO move cursor - update font family, size, color, ... in editor

    private void FontSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count == 0) return;

        if (Selection.IsEmpty) return; //TODO

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
                saveMenuItem.Command.Execute(null);
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
        var textDataFormat = extension == ".rtf" ? TextDataFormat.Rtf : TextDataFormat.Text;
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
}
