using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace WpfRtfEditor;

public class RtfSelectionState
{
    public FontFamily FontFamily { get; set; }
    public double FontSize { get; set; }

    public override bool Equals(object? obj)
    {
        return obj is RtfSelectionState state &&
               EqualityComparer<FontFamily>.Default.Equals(FontFamily, state.FontFamily) &&
               FontSize == state.FontSize;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(FontFamily, FontSize);
    }
}
