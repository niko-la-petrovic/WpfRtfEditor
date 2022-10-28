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

namespace WpfRtfEditor;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    protected bool ApplyStylingNext { get; set; }
    protected RtfSelectionState RtfSelectionState { get; set; }

    // TODO use memo

    public MainWindow()
    {
        InitializeComponent();

        var fonts = Fonts.SystemFontFamilies.OrderBy(f => f.Source);
        var fontSizes = new List<double>() { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 };
        RtfSelectionState = new RtfSelectionState
        {
            FontFamily = fonts.FirstOrDefault(f => f.Source == "Calibri"),
            FontSize = 11
        };

        fontComboBox.ItemsSource = fonts;
        fontComboBox.SelectedItem = RtfSelectionState.FontFamily; // TODO not doing its job

        fontSizeComboBox.ItemsSource = fonts;
        fontSizeComboBox.SelectedItem = RtfSelectionState.FontSize; // TODO not doing its job
    }

    private void FontComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // TODO
        if (e.AddedItems.Count == 0) return;

        if (rtfTextBox.Selection.IsEmpty)
        {
            RtfSelectionState.FontFamily = fontComboBox.SelectedItem as FontFamily;
            ApplyStylingNext = true;
            return;
        }

        rtfTextBox.Selection.ApplyPropertyValue(TextElement.FontFamilyProperty, fontComboBox.SelectedItem);
        // TODO extract
        ApplyStylingNext = false;
        //rtfTextBox.FontFamily = e.AddedItems[0] as FontFamily;
    }

    private void RtfTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        //rtfTextBox.Selection.Select(rtfTextBox.CaretPosition, rtfTextBox.CaretPosition.GetPositionAtOffset(-1));
        //// TODO only do this if unhandled property change
        //rtfTextBox.Selection.ApplyPropertyValue(TextElement.FontFamilyProperty, fontComboBox.SelectedItem);

        //rtfTextBox.Selection.Select(rtfTextBox.CaretPosition.DocumentEnd, rtfTextBox.CaretPosition.DocumentEnd);


        //rtfTextBox.Selection.Select()
        //
    }

    // TODO move cursor - update font family, size, color, ... in editor

    private void FontSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // TODO
        if (e.AddedItems.Count == 0) return;

        rtfTextBox.FontSize = (double)e.AddedItems[0];
    }

    private void PasteButton_Click(object sender, RoutedEventArgs e)
    {
        // TODO disable button if clipboard empty?
        //if(Clipboard.ContainsData() // TODO check predefined formats
        if (Clipboard.ContainsText())
        {
            var text = Clipboard.GetText();
            rtfTextBox.AppendText(text);
            return;
        }

        if (Clipboard.ContainsData(DataFormats.Rtf))
        {
            var rtfText = Clipboard.GetText(TextDataFormat.Rtf);
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(rtfText));
            rtfTextBox.Selection.Load(ms, DataFormats.Rtf);
            return;
        }
    }

    private void CutButton_Click(object sender, RoutedEventArgs e)
    {
        // TODO disable button if no selection
        if (rtfTextBox.Selection.IsEmpty) return;
        // TODO handle
    }

    private void CopyButton_Click(object sender, RoutedEventArgs e)
    {
        // TODO disable button if no selection
        if (rtfTextBox.Selection.IsEmpty) return;

        var rtfDataFormat = DataFormats.Rtf;
        if (rtfTextBox.Selection.CanSave(rtfDataFormat))
        {
            using var ms = new MemoryStream();
            rtfTextBox.Selection.Save(ms, rtfDataFormat);
            Clipboard.SetData(rtfDataFormat, ms.ToArray());
        }

        // TODO other types
    }

    private void RtfTextBox_SelectionChanged(object sender, RoutedEventArgs e)
    {

    }

    private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
    {

    }

    private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {

    }
}
