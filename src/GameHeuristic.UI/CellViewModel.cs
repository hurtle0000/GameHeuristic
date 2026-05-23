using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Media;

namespace GameHeuristic.UI;

public class CellViewModel : INotifyPropertyChanged
{
    private IBrush _color = Brushes.White;

    public IBrush Color
    {
        get => _color;
        set
        {
            if (_color != value)
            {
                _color = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
