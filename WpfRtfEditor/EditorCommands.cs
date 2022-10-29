using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WpfRtfEditor;

public static class EditorCommands
{
    public static RoutedUICommand StrikeThroughCommand { get; } = new();
    public static RoutedUICommand FontColorCommand { get; } = new();
    public static RoutedUICommand FontHighlightCommand { get; } = new();
    public static RoutedUICommand ClearFormatting { get; } = new();
    // TODO
    //public static RoutedUICommand FontFamilyCommand = new RoutedUICommand();
    //public static RoutedUICommand FontSizeCommand = new RoutedUICommand();
}
