using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GameHeuristic.UI;

public class ParticipantViewModel : INotifyPropertyChanged
{
    private bool _isSelected = true;
    public string Name { get; set; } = string.Empty;
    public GameHeuristic.Core.IHeuristic Heuristic { get; set; } = null!;

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
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

public class TournamentResultViewModel : INotifyPropertyChanged
{
    private int _rank;
    private int _wins;
    private int _losses;
    private int _draws;
    private double _score;

    public int Rank { get => _rank; set { _rank = value; OnPropertyChanged(); } }
    public string Name { get; set; } = string.Empty;
    public int Wins { get => _wins; set { _wins = value; OnPropertyChanged(); } }
    public int Losses { get => _losses; set { _losses = value; OnPropertyChanged(); } }
    public int Draws { get => _draws; set { _draws = value; OnPropertyChanged(); } }
    public double Score { get => _score; set { _score = value; OnPropertyChanged(); } }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
