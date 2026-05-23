using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.Controls.Shapes;
using GameHeuristic.Core;

namespace GameHeuristic.UI;

public partial class MainWindow : Window
{
    // Core Game Fields
    private Board _board = new Board();
    private readonly Ellipse[,] _uiCells = new Ellipse[Board.Rows, Board.Columns];
    private List<IHeuristic> _singleMatchHeuristics = new List<IHeuristic>();
    private List<IHeuristic> _tournamentHeuristics = new List<IHeuristic>();

    // Single Match Timer Loop
    private DispatcherTimer? _gameTimer;
    private MinimaxAI? _ai1;
    private MinimaxAI? _ai2;
    private Player _currentPlayer;

    // Tournament Fields
    private CancellationTokenSource? _tournamentCts;
    private List<TournamentResult> _results = new List<TournamentResult>();

    public MainWindow()
    {
        InitializeComponent();
        InitializeBoard();
        LoadGroups();
        LoadHeuristics();
    }

    /// <summary>
    /// Procedurally builds the Connect 4 grid of Ellipse elements in the UI
    /// and stores references in a 2D array for simple, direct manipulation.
    /// </summary>
    private void InitializeBoard()
    {
        BoardGrid.Children.Clear();
        for (int r = 0; r < Board.Rows; r++)
        {
            for (int c = 0; c < Board.Columns; c++)
            {
                var ellipse = new Ellipse
                {
                    Width = 60,
                    Height = 60,
                    Margin = new Thickness(5),
                    Stroke = Brushes.Black,
                    StrokeThickness = 1,
                    Fill = Brushes.White
                };
                BoardGrid.Children.Add(ellipse);
                _uiCells[r, c] = ellipse;
            }
        }
    }

    /// <summary>
    /// Loads Heuristic Group namespaces dynamically (Baselines, Teacher, etc.)
    /// </summary>
    private void LoadGroups()
    {
        List<string> groups = new List<string> { "All" };
        groups.AddRange(HeuristicLoader.GetAvailableGroups());

        GroupFilterCombo.ItemsSource = groups;
        GroupFilterCombo.SelectedIndex = 0;

        TournamentGroupFilterCombo.ItemsSource = groups;
        TournamentGroupFilterCombo.SelectedIndex = 0;
    }

    private void LoadHeuristics()
    {
        LoadHeuristicsForSingleMatch("All");
        LoadHeuristicsForTournament("All");
    }

    private void LoadHeuristicsForSingleMatch(string group)
    {
        _singleMatchHeuristics = HeuristicLoader.LoadHeuristics(group);
        
        Player1Combo.ItemsSource = _singleMatchHeuristics.Select(h => h.Name).ToList();
        Player2Combo.ItemsSource = _singleMatchHeuristics.Select(h => h.Name).ToList();

        if (_singleMatchHeuristics.Count > 0)
        {
            Player1Combo.SelectedIndex = 0;
            Player2Combo.SelectedIndex = Math.Min(1, _singleMatchHeuristics.Count - 1);
        }
    }

    /// <summary>
    /// Dynamically populates the participants StackPanel with CheckBoxes
    /// </summary>
    private void LoadHeuristicsForTournament(string group)
    {
        _tournamentHeuristics = HeuristicLoader.LoadHeuristics(group);
        ParticipantList.Children.Clear();

        foreach (IHeuristic h in _tournamentHeuristics)
        {
            var checkBox = new CheckBox
            {
                Content = h.Name,
                IsChecked = true,
                Tag = h,
                Margin = new Thickness(5, 2)
            };
            ParticipantList.Children.Add(checkBox);
        }
    }

    private void OnGroupFilterChanged(object sender, SelectionChangedEventArgs e)
    {
        if (GroupFilterCombo != null && GroupFilterCombo.SelectedItem is string selectedGroup)
        {
            LoadHeuristicsForSingleMatch(selectedGroup);
        }
    }

    private void OnTournamentGroupFilterChanged(object sender, SelectionChangedEventArgs e)
    {
        if (TournamentGroupFilterCombo != null && TournamentGroupFilterCombo.SelectedItem is string selectedGroup)
        {
            LoadHeuristicsForTournament(selectedGroup);
        }
    }

    #region Single Match Logic

    /// <summary>
    /// Starts the single match timer loop (Single-Threaded, Event-Driven)
    /// </summary>
    private void OnStartClick(object sender, RoutedEventArgs e)
    {
        if (_gameTimer != null) return;

        ResetBoard();
        StartButton.IsEnabled = false;
        
        IHeuristic h1 = _singleMatchHeuristics[Player1Combo.SelectedIndex];
        IHeuristic h2 = _singleMatchHeuristics[Player2Combo.SelectedIndex];

        _ai1 = new MinimaxAI(h1, depth: 6);
        _ai2 = new MinimaxAI(h2, depth: 6);
        _currentPlayer = Player.Red;

        StatusText.Text = "Red's turn...";

        // Start the single-threaded game event loop
        _gameTimer = new DispatcherTimer();
        _gameTimer.Interval = TimeSpan.FromMilliseconds(Math.Max(50, DelaySlider.Value));
        _gameTimer.Tick += GameTimer_Tick;
        _gameTimer.Start();
    }

    /// <summary>
    /// The timer tick represents a single game turn.
    /// Runs AI searches on a background task so the Avalonia UI remains fully responsive.
    /// </summary>
    private void GameTimer_Tick(object? sender, EventArgs e)
    {
        _gameTimer?.Stop(); // Temporarily stop to prevent overlapping ticks

        MinimaxAI activeAi = _currentPlayer == Player.Red ? _ai1! : _ai2!;

        // Clear UI scores before starting new calculation
        ClearScoresUI();

        // Subscribe to live column score updates
        activeAi.OnColumnEvaluated = (col, score) =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                UpdateScoreUI(col, score);
            });
        };

        // Capture current state to ensure thread-safety if reset during execution
        Board startBoard = _board;
        Player startPlayer = _currentPlayer;

        StatusText.Text = $"{(_currentPlayer == Player.Red ? "Red" : "Yellow")} is thinking...";

        Task.Run(() =>
        {
            int move = activeAi.GetBestMove(startBoard, startPlayer);

            Dispatcher.UIThread.Post(() =>
            {
                // Verify that game hasn't been reset in the meantime
                if (_board != startBoard || _currentPlayer != startPlayer || _gameTimer == null)
                    return;

                if (move == -1 || !_board.MakeMove(move, _currentPlayer))
                {
                    StatusText.Text = $"{(_currentPlayer == Player.Red ? "Red" : "Yellow")} forfeited the game!";
                    CleanupGame();
                    return;
                }

                UpdateBoardUI();

                GameState state = _board.CheckGameState();
                if (state != GameState.Ongoing)
                {
                    if (state == GameState.Draw)
                        StatusText.Text = "Game ended in a draw!";
                    else
                    {
                        StatusText.Text = $"{(_currentPlayer == Player.Red ? "Red" : "Yellow")} wins!";
                        HighlightWinningLine();
                    }
                    CleanupGame();
                    return;
                }

                // Switch turns
                _currentPlayer = _currentPlayer == Player.Red ? Player.Yellow : Player.Red;
                StatusText.Text = $"{(_currentPlayer == Player.Red ? "Red" : "Yellow")}'s turn...";

                // Restart timer, dynamically reading the interval in case the user adjusted the slider
                if (_gameTimer != null)
                {
                    _gameTimer.Interval = TimeSpan.FromMilliseconds(Math.Max(50, DelaySlider.Value));
                    _gameTimer.Start();
                }
            });
        });
    }

    private void UpdateScoreUI(int col, double score)
    {
        TextBlock[] labels = { ColScore0, ColScore1, ColScore2, ColScore3, ColScore4, ColScore5, ColScore6 };
        if (col < 0 || col >= labels.Length) return;

        if (double.IsNaN(score))
        {
            labels[col].Text = "-";
            labels[col].Foreground = Brushes.LightGray;
        }
        else
        {
            string scoreText = score.ToString("F1");
            if (score > 900000)
            {
                scoreText = "WIN";
                labels[col].Foreground = Brushes.LightGreen;
            }
            else if (score < -900000)
            {
                scoreText = "LOSS";
                labels[col].Foreground = Brushes.OrangeRed;
            }
            else
            {
                labels[col].Foreground = score > 0 ? Brushes.LightGreen : (score < 0 ? Brushes.LightPink : Brushes.White);
            }
            labels[col].Text = scoreText;
        }
    }

    private void ClearScoresUI()
    {
        TextBlock[] labels = { ColScore0, ColScore1, ColScore2, ColScore3, ColScore4, ColScore5, ColScore6 };
        foreach (var lbl in labels)
        {
            lbl.Text = "";
        }
    }

    private void HighlightWinningLine()
    {
        var line = _board.GetWinningLine();
        foreach (var cell in line)
        {
            int r = cell.Row;
            int c = cell.Col;
            if (r >= 0 && r < Board.Rows && c >= 0 && c < Board.Columns)
            {
                _uiCells[r, c].Stroke = Brushes.Gold;
                _uiCells[r, c].StrokeThickness = 4;
            }
        }
    }

    private void CleanupGame()
    {
        _gameTimer?.Stop();
        _gameTimer = null;
        StartButton.IsEnabled = true;
    }

    private void OnResetClick(object sender, RoutedEventArgs e)
    {
        CleanupGame();
        ResetBoard();
        StatusText.Text = "Select players and start";
    }

    private void ResetBoard()
    {
        _board = new Board();
        ClearScoresUI();
        for (int r = 0; r < Board.Rows; r++)
        {
            for (int c = 0; c < Board.Columns; c++)
            {
                _uiCells[r, c].Stroke = Brushes.Black;
                _uiCells[r, c].StrokeThickness = 1;
            }
        }
        UpdateBoardUI();
    }

    /// <summary>
    /// Procedural drawing of grid cells based on the underlying board array
    /// </summary>
    private void UpdateBoardUI()
    {
        for (int r = 0; r < Board.Rows; r++)
        {
            for (int c = 0; c < Board.Columns; c++)
            {
                Player piece = _board.GetPiece(r, c);
                _uiCells[r, c].Fill = piece switch
                {
                    Player.Red => Brushes.Red,
                    Player.Yellow => Brushes.Yellow,
                    _ => Brushes.White
                };
            }
        }
    }

    #endregion

    #region Tournament Logic

    private async void OnRunTournamentClick(object sender, RoutedEventArgs e)
    {
        // Gather selected participants by iterating our CheckBox collection
        List<IHeuristic> selected = new List<IHeuristic>();
        foreach (var child in ParticipantList.Children)
        {
            if (child is CheckBox cb && cb.IsChecked == true && cb.Tag is IHeuristic h)
            {
                selected.Add(h);
            }
        }

        if (selected.Count < 2)
        {
            TournamentStatus.Text = "Select at least 2 participants";
            return;
        }

        _tournamentCts = new CancellationTokenSource();
        RunTournamentButton.IsEnabled = false;
        StopTournamentButton.IsEnabled = true;

        // Initialize plain C# results list
        _results = selected.Select(h => new TournamentResult { Name = h.Name }).ToList();
        UpdateLeaderboardUI(_results);

        int iterations = (int)(IterationCount.Value ?? 1);
        int depth = (int)(TournamentDepth.Value ?? 4);
        bool isRoundRobin = TournamentModeCombo.SelectedIndex == 0;

        try
        {
            // Background thread is used solely to prevent freezing during high-iteration matches
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

    private void RunTournament(List<IHeuristic> participants, int iterations, int depth, bool isRoundRobin, CancellationToken ct)
    {
        Dictionary<string, TournamentResult> stats = _results.ToDictionary(r => r.Name);
        int totalMatches = isRoundRobin 
            ? (participants.Count * (participants.Count - 1) * iterations) 
            : CalculateKnockoutMatches(participants.Count, iterations);
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

                        Player winner = PlayHeadlessGame(participants[i], participants[j], depth);
                        UpdateStats(stats, participants[i].Name, participants[j].Name, winner);
                        
                        matchesPlayed++;
                        UpdateProgress(matchesPlayed, totalMatches);
                    }
                }
            }
            else // Knockout
            {
                List<IHeuristic> currentRound = new List<IHeuristic>(participants);
                while (currentRound.Count > 1)
                {
                    ct.ThrowIfCancellationRequested();
                    List<IHeuristic> winners = new List<IHeuristic>();
                    
                    for (int i = 0; i < currentRound.Count - 1; i += 2)
                    {
                        ct.ThrowIfCancellationRequested();
                        IHeuristic h1 = currentRound[i];
                        IHeuristic h2 = currentRound[i + 1];
                        
                        Player winner = PlayHeadlessGame(h1, h2, depth);
                        UpdateStats(stats, h1.Name, h2.Name, winner);
                        
                        if (winner == Player.Yellow) winners.Add(h2);
                        else winners.Add(h1);

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

    private void UpdateStats(Dictionary<string, TournamentResult> stats, string redName, string yellowName, Player winner)
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

        // Sort results by Score and Wins, recalculate Rank, and push straight to GUI
        List<TournamentResult> sorted = _results.OrderByDescending(r => r.Score).ThenByDescending(r => r.Wins).ToList();
        for (int i = 0; i < sorted.Count; i++)
        {
            sorted[i].Rank = i + 1;
        }

        Dispatcher.UIThread.Post(() => UpdateLeaderboardUI(sorted));
    }

    /// <summary>
    /// Procedurally clears and rebuilds grid-based rows for the Leaderboard.
    /// This is 100% direct and eliminates complex dynamic layout binding templates.
    /// </summary>
    private void UpdateLeaderboardUI(List<TournamentResult> results)
    {
        LeaderboardList.Children.Clear();
        foreach (var res in results)
        {
            var grid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("60, *, 80, 80, 80, 100"),
                Margin = new Thickness(0, 2)
            };

            var rankText = new TextBlock { Text = res.Rank.ToString(), Margin = new Thickness(5) };
            var nameText = new TextBlock { Text = res.Name, Margin = new Thickness(5) };
            var winsText = new TextBlock { Text = res.Wins.ToString(), Margin = new Thickness(5) };
            var lossesText = new TextBlock { Text = res.Losses.ToString(), Margin = new Thickness(5) };
            var drawsText = new TextBlock { Text = res.Draws.ToString(), Margin = new Thickness(5) };
            var scoreText = new TextBlock { Text = res.Score.ToString("F1"), Margin = new Thickness(5) };

            Grid.SetColumn(rankText, 0);
            Grid.SetColumn(nameText, 1);
            Grid.SetColumn(winsText, 2);
            Grid.SetColumn(lossesText, 3);
            Grid.SetColumn(drawsText, 4);
            Grid.SetColumn(scoreText, 5);

            grid.Children.Add(rankText);
            grid.Children.Add(nameText);
            grid.Children.Add(winsText);
            grid.Children.Add(lossesText);
            grid.Children.Add(drawsText);
            grid.Children.Add(scoreText);

            LeaderboardList.Children.Add(grid);
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

/// <summary>
/// A plain C# data structure representing a single competitor's score stats.
/// Zero MVVM, INotifyPropertyChanged, or complex boilerplate.
/// </summary>
public class TournamentResult
{
    public int Rank { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int Draws { get; set; }
    public double Score => Wins + (Draws * 0.5);
}
