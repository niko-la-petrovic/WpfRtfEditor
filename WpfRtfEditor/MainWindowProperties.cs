using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace WpfRtfEditor;

public class MainWindowProperties : INotifyPropertyChanged
{
    private Brush backgroundTextBrush = new SolidColorBrush(Colors.Transparent);

    private Brush foregroundTextBrush = new SolidColorBrush(Colors.Black);

    public event PropertyChangedEventHandler? PropertyChanged;

    public Brush BackgroundTextBrush
    {
        get => backgroundTextBrush; set
        {
            backgroundTextBrush = value;
            OnPropertyChanged();
        }
    }

    public Brush ForegroundTextBrush
    {
        get => foregroundTextBrush; set
        {
            foregroundTextBrush = value;
            OnPropertyChanged();
        }
    }

    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
