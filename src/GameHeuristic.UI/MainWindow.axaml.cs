using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using GameHeuristic.Core;

namespace GameHeuristic.UI;

public partial class MainWindow : Window
{
    private Board _board = new Board();
    private ObservableCollection<CellViewModel> _cells = new ObservableCollection<CellViewModel>();
    private CancellationTokenSource? _gameCts;
    private List<IHeuristic> _heuristics = new List<IHeuristic>();
    
    // Tournament fields
    private ObservableCollection<ParticipantViewModel> _participants = new ObservableCollection<ParticipantViewModel>();
    private ObservableCollection<TournamentResultViewModel> _results = new ObservableCollection<TournamentResultViewModel>();
    private CancellationTokenSource? _tournamentCts;

    public MainWindow()
    {
        InitializeComponent();
        InitializeBoard();
        LoadHeuristics();
    }

    private void InitializeBoard()
    {
        _cells.Clear();
        for (int r = 0; r < Board.Rows; r++)
        {
            for (int c = 0; c < Board.Columns; c++)
            {
                _cells.Add(new CellViewModel());
            }
        }
        BoardDisplay.ItemsSource = _cells;
    }

    private void LoadHeuristics()
    {
        _heuristics = HeuristicLoader.LoadHeuristics();
        
        // Single Match setup
        Player1Combo.ItemsSource = _heuristics.Select(h => h.Name).ToList();
        Player2Combo.ItemsSource = _heuristics.Select(h => h.Name).ToList();

        if (_heuristics.Count > 0)
        {
            Player1Combo.SelectedIndex = 0;
            Player2Combo.SelectedIndex = Math.Min(1, _heuristics.Count - 1);
        }

        // Tournament setup
        _participants.Clear();
        foreach (IHeuristic h in _heuristics)
        {
            _participants.Add(new ParticipantViewModel { Name = h.Name, Heuristic = h });
        }
        ParticipantList.ItemsSource = _participants;
        LeaderboardList.ItemsSource = _results;
    }

    #region Single Match Logic
    private async void OnStartClick(object sender, RoutedEventArgs e)
    {
        if (_gameCts != null) return;

        ResetBoard();
        _gameCts = new CancellationTokenSource();
        StartButton.IsEnabled = false;
        
        IHeuristic h1 = _heuristics[Player1Combo.SelectedIndex];
        IHeuristic h2 = _heuristics[Player2Combo.SelectedIndex];

        try
        {
            await RunGame(h1, h2, _gameCts.Token);
        }
        catch (OperationCanceledException)
        {
            StatusText.Text = "Game Reset";
        }
        finally
        {
            _gameCts = null;
            StartButton.IsEnabled = true;
        }
    }

    private async Task RunGame(IHeuristic h1, IHeuristic h2, CancellationToken ct)
    {
        MinimaxAI ai1 = new MinimaxAI(h1);
        MinimaxAI ai2 = new MinimaxAI(h2);
        Player currentPlayer = Player.Red;

        while (true)
        {
            ct.ThrowIfCancellationRequested();

            StatusText.Text = $"{(currentPlayer == Player.Red ? h1.Name : h2.Name)}'s turn ({currentPlayer})";
            
            int move = await Task.Run(() => 
                (currentPlayer == Player.Red ? ai1 : ai2).GetBestMove(_board, currentPlayer), ct);

            if (move == -1 || !_board.MakeMove(move, currentPlayer))
            {
                StatusText.Text = $"{currentPlayer} made an invalid move!";
                break;
            }

            UpdateBoardUI();

            GameState state = _board.CheckGameState();
            if (state != GameState.Ongoing)
            {
                StatusText.Text = state switch
                {
                    GameState.RedWin => $"{h1.Name} (Red) Wins!",
                    GameState.YellowWin => $"{h2.Name} (Yellow) Wins!",
                    GameState.Draw => "It's a Draw!",
                    _ => ""
                };
                break;
            }

            currentPlayer = currentPlayer == Player.Red ? Player.Yellow : Player.Red;
            await Task.Delay((int)DelaySlider.Value, ct);
        }
    }

    private void UpdateBoardUI()
    {
        for (int r = 0; r < Board.Rows; r++)
        {
            for (int c = 0; c < Board.Columns; c++)
            {
                Player piece = _board.GetPiece(r, c);
                _cells[r * Board.Columns + c].Color = piece switch
                {
                    Player.Red => Brushes.Red,
                    Player.Yellow => Brushes.Yellow,
                    _ => Brushes.White
                };
            }
        }
    }

    private void OnResetClick(object sender, RoutedEventArgs e)
    {
        _gameCts?.Cancel();
        ResetBoard();
        StatusText.Text = "Select players and start";
    }

    private void ResetBoard()
    {
        _board = new Board();
        UpdateBoardUI();
    }
    #endregion

    #region Tournament Logic
    private async void OnRunTournamentClick(object sender, RoutedEventArgs e)
    {
        List<ParticipantViewModel> selected = _participants.Where(p => p.IsSelected).ToList();
        if (selected.Count < 2)
        {
            TournamentStatus.Text = "Select at least 2 participants";
            return;
        }

        _tournamentCts = new CancellationTokenSource();
        RunTournamentButton.IsEnabled = false;
        StopTournamentButton.IsEnabled = true;
        _results.Clear();
        foreach (ParticipantViewModel p in selected)
        {
            _results.Add(new TournamentResultViewModel { Name = p.Name });
        }

        int iterations = (int)(IterationCount.Value ?? 1);
        int depth = (int)(TournamentDepth.Value ?? 4);
        bool isRoundRobin = TournamentModeCombo.SelectedIndex == 0;

        try
        {
            await Task.Run(() => RunTournament(selected, iterations, depth, isRoundRobin, _tournamentCts.Token));
            TournamentStatus.Text = "Tournament Finished";
        }
        catch (OperationCanceledException)
        {
            TournamentStatus.Text = "Tournament Stopped";
        }
        finally
        {
            _tournamentCts = null;
            RunTournamentButton.IsEnabled = true;
            StopTournamentButton.IsEnabled = false;
        }
    }

    private void OnStopTournamentClick(object sender, RoutedEventArgs e)
    {
        _tournamentCts?.Cancel();
    }

    private void RunTournament(List<ParticipantViewModel> participants, int iterations, int depth, bool isRoundRobin, CancellationToken ct)
    {
        Dictionary<string, TournamentResultViewModel> stats = _results.ToDictionary(r => r.Name);
        int totalMatches = isRoundRobin ? (participants.Count * (participants.Count - 1) * iterations) : CalculateKnockoutMatches(participants.Count, iterations);
        int matchesPlayed = 0;

        for (int iter = 0; iter < iterations; iter++)
        {
            ct.ThrowIfCancellationRequested();

            if (isRoundRobin)
            {
                for (int i = 0; i < participants.Count; i++)
                {
                    for (int j = 0; j < participants.Count; j++)
                    {
                        if (i == j) continue;
                        ct.ThrowIfCancellationRequested();

                        Player winner = PlayHeadlessGame(participants[i].Heuristic, participants[j].Heuristic, depth);
                        UpdateStats(stats, participants[i].Name, participants[j].Name, winner);
                        
                        matchesPlayed++;
                        UpdateProgress(matchesPlayed, totalMatches);
                    }
                }
            }
            else // Knockout
            {
                List<ParticipantViewModel> currentRound = new List<ParticipantViewModel>(participants);
                while (currentRound.Count > 1)
                {
                    ct.ThrowIfCancellationRequested();
                    List<ParticipantViewModel> winners = new List<ParticipantViewModel>();
                    
                    for (int i = 0; i < currentRound.Count - 1; i += 2)
                    {
                        ct.ThrowIfCancellationRequested();
                        IHeuristic h1 = currentRound[i].Heuristic;
                        IHeuristic h2 = currentRound[i + 1].Heuristic;
                        
                        Player winner = PlayHeadlessGame(h1, h2, depth);
                        UpdateStats(stats, currentRound[i].Name, currentRound[i + 1].Name, winner);
                        
                        if (winner == Player.Yellow) winners.Add(currentRound[i + 1]);
                        else winners.Add(currentRound[i]); // Red wins or Draw (Red proceeds on draw for knockout simplicity)

                        matchesPlayed++;
                        UpdateProgress(matchesPlayed, totalMatches);
                    }
                    if (currentRound.Count % 2 != 0) winners.Add(currentRound.Last());
                    currentRound = winners;
                }
            }
        }
    }

    private int CalculateKnockoutMatches(int participantCount, int iterations)
    {
        // Simple approximation for progress bar
        int matchesPerIter = 0;
        int count = participantCount;
        while (count > 1)
        {
            matchesPerIter += count / 2;
            count = (count / 2) + (count % 2);
        }
        return matchesPerIter * iterations;
    }

    private Player PlayHeadlessGame(IHeuristic h1, IHeuristic h2, int depth)
    {
        Board board = new Board();
        MinimaxAI ai1 = new MinimaxAI(h1, depth);
        MinimaxAI ai2 = new MinimaxAI(h2, depth);
        Player currentPlayer = Player.Red;

        while (true)
        {
            int move = (currentPlayer == Player.Red ? ai1 : ai2).GetBestMove(board, currentPlayer);
            if (move == -1 || !board.MakeMove(move, currentPlayer))
            {
                return currentPlayer == Player.Red ? Player.Yellow : Player.Red;
            }

            GameState state = board.CheckGameState();
            if (state != GameState.Ongoing)
            {
                if (state == GameState.Draw) return Player.None;
                return state == GameState.RedWin ? Player.Red : Player.Yellow;
            }
            currentPlayer = currentPlayer == Player.Red ? Player.Yellow : Player.Red;
        }
    }

    private void UpdateStats(Dictionary<string, TournamentResultViewModel> stats, string redName, string yellowName, Player winner)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (winner == Player.Red)
            {
                stats[redName].Wins++;
                stats[yellowName].Losses++;
            }
            else if (winner == Player.Yellow)
            {
                stats[yellowName].Wins++;
                stats[redName].Losses++;
            }
            else
            {
                stats[redName].Draws++;
                stats[yellowName].Draws++;
            }
            
            stats[redName].Score = stats[redName].Wins + (stats[redName].Draws * 0.5);
            stats[yellowName].Score = stats[yellowName].Wins + (stats[yellowName].Draws * 0.5);
            
            UpdateLeaderboard();
        });
    }

    private void UpdateLeaderboard()
    {
        List<TournamentResultViewModel> sorted = _results.OrderByDescending(r => r.Score).ThenByDescending(r => r.Wins).ToList();
        for (int i = 0; i < sorted.Count; i++)
        {
            TournamentResultViewModel item = sorted[i];
            item.Rank = i + 1;
            
            int oldIndex = _results.IndexOf(item);
            if (oldIndex != i)
            {
                _results.Move(oldIndex, i);
            }
        }
    }

    private void UpdateProgress(int played, int total)
    {
        Dispatcher.UIThread.Post(() =>
        {
            TournamentProgress.Value = (double)played / total * 100;
            TournamentStatus.Text = $"Matches: {played} / {total}";
        });
    }
    #endregion
}
