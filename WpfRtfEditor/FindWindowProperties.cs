using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WpfRtfEditor;

public class FindWindowProperties : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<string> LastSearches { get; } = new();

    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public void AddLastSearch(string lastSearch)
    {
        if (string.IsNullOrWhiteSpace(lastSearch))
            return;

        if (LastSearches.Contains(lastSearch))
            return;

        LastSearches.Add(lastSearch);
        OnPropertyChanged(nameof(LastSearches));
    }
}
