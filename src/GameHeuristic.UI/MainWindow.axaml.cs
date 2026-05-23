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

        // Clear bracket canvas at start
        BracketCanvas.Children.Clear();

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
                int roundIndex = 0;
                List<KnockoutMatch> matchesList = new List<KnockoutMatch>();

                while (currentRound.Count > 1)
                {
                    ct.ThrowIfCancellationRequested();
                    List<IHeuristic> winners = new List<IHeuristic>();
                    int matchIndex = 0;
                    
                    for (int i = 0; i < currentRound.Count - 1; i += 2)
                    {
                        ct.ThrowIfCancellationRequested();
                        IHeuristic h1 = currentRound[i];
                        IHeuristic h2 = currentRound[i + 1];
                        
                        Player winner = PlayHeadlessGame(h1, h2, depth);
                        UpdateStats(stats, h1.Name, h2.Name, winner);
                        
                        string winnerName = winner == Player.Yellow ? h2.Name : h1.Name;
                        if (winner == Player.Yellow) winners.Add(h2);
                        else winners.Add(h1);

                        matchesList.Add(new KnockoutMatch
                        {
                            Player1 = h1.Name,
                            Player2 = h2.Name,
                            Winner = winnerName,
                            RoundIndex = roundIndex,
                            MatchIndex = matchIndex
                        });

                        matchIndex++;
                        matchesPlayed++;
                        UpdateProgress(matchesPlayed, totalMatches);
                    }
                    if (currentRound.Count % 2 != 0)
                    {
                        var byePlayer = currentRound.Last();
                        winners.Add(byePlayer);
                        
                        matchesList.Add(new KnockoutMatch
                        {
                            Player1 = byePlayer.Name,
                            Player2 = "(BYE)",
                            Winner = byePlayer.Name,
                            RoundIndex = roundIndex,
                            MatchIndex = matchIndex
                        });
                        matchIndex++;
                    }
                    currentRound = winners;
                    roundIndex++;
                }

                // Render knockout bracket in the UI thread
                Dispatcher.UIThread.Post(() => RenderKnockoutBracket(matchesList));
            }
        }
    }

    private void RenderKnockoutBracket(List<KnockoutMatch> matches)
    {
        BracketCanvas.Children.Clear();
        if (matches.Count == 0) return;

        int maxRound = matches.Max(m => m.RoundIndex);
        
        // Calculate and set canvas size dynamically to allow scrolling
        double canvasWidth = (maxRound + 1) * 260 + 100;
        double canvasHeight = (1 << maxRound) * 120 + 100;
        BracketCanvas.Width = Math.Max(1000, canvasWidth);
        BracketCanvas.Height = Math.Max(600, canvasHeight);

        // Standard measurements
        double nodeWidth = 180;
        double nodeHeight = 55;

        // Draw connections first so they render under the nodes
        foreach (var match in matches)
        {
            if (match.RoundIndex < maxRound)
            {
                // Find coordinates of this node's right center
                double sCurrent = 70 * Math.Pow(2, match.RoundIndex);
                double yCurrent = match.MatchIndex * sCurrent + (sCurrent / 2);
                double xCurrentRight = match.RoundIndex * 260 + 40 + nodeWidth;

                // Find coordinates of the child node's left center
                int nextRound = match.RoundIndex + 1;
                int nextMatchIndex = match.MatchIndex / 2;
                double sNext = 70 * Math.Pow(2, nextRound);
                double yNext = nextMatchIndex * sNext + (sNext / 2);
                double xNextLeft = nextRound * 260 + 40;

                // Draw neat orthogonal connection line (Right -> MidpointX -> MidpointX -> YNext -> XNext)
                double midX = xCurrentRight + (xNextLeft - xCurrentRight) / 2;

                var line1 = new Line
                {
                    StartPoint = new Point(xCurrentRight, yCurrent),
                    EndPoint = new Point(midX, yCurrent),
                    Stroke = Brushes.SlateGray,
                    StrokeThickness = 2
                };
                var line2 = new Line
                {
                    StartPoint = new Point(midX, yCurrent),
                    EndPoint = new Point(midX, yNext),
                    Stroke = Brushes.SlateGray,
                    StrokeThickness = 2
                };
                var line3 = new Line
                {
                    StartPoint = new Point(midX, yNext),
                    EndPoint = new Point(xNextLeft, yNext),
                    Stroke = Brushes.SlateGray,
                    StrokeThickness = 2
                };

                BracketCanvas.Children.Add(line1);
                BracketCanvas.Children.Add(line2);
                BracketCanvas.Children.Add(line3);
            }
        }

        // Draw nodes
        foreach (var match in matches)
        {
            double s = 70 * Math.Pow(2, match.RoundIndex);
            double y = match.MatchIndex * s + (s / 2);
            double x = match.RoundIndex * 260 + 40;

            double top = y - (nodeHeight / 2);

            // Create a border panel representing the match box
            var matchBorder = new Border
            {
                Width = nodeWidth,
                Height = nodeHeight,
                Background = new SolidColorBrush(Color.Parse("#2e3047")),
                BorderBrush = new SolidColorBrush(Color.Parse("#414561")),
                BorderThickness = new Thickness(1.5),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(6, 4)
            };

            // Setup a grid layout inside the node box
            var grid = new Grid
            {
                RowDefinitions = new RowDefinitions("*, *"),
                ColumnDefinitions = new ColumnDefinitions("*, Auto")
            };

            // Player 1 details
            var p1Text = new TextBlock
            {
                Text = ShortenName(match.Player1),
                FontSize = 11,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };
            var p1Status = new TextBlock
            {
                Text = match.Winner == match.Player1 && match.Player1 != "(BYE)" ? "🏆" : "",
                FontSize = 10,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };
            if (match.Winner == match.Player1)
            {
                p1Text.Foreground = Brushes.LightGreen;
                p1Text.FontWeight = FontWeight.Bold;
            }
            else
            {
                p1Text.Foreground = match.Player2 == "(BYE)" ? Brushes.LightGreen : Brushes.Gray;
            }

            Grid.SetRow(p1Text, 0);
            Grid.SetColumn(p1Text, 0);
            Grid.SetRow(p1Status, 0);
            Grid.SetColumn(p1Status, 1);
            grid.Children.Add(p1Text);
            grid.Children.Add(p1Status);

            // Player 2 details
            var p2Text = new TextBlock
            {
                Text = ShortenName(match.Player2),
                FontSize = 11,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };
            var p2Status = new TextBlock
            {
                Text = match.Winner == match.Player2 && match.Player2 != "(BYE)" ? "🏆" : "",
                FontSize = 10,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };
            if (match.Winner == match.Player2)
            {
                p2Text.Foreground = Brushes.LightGreen;
                p2Text.FontWeight = FontWeight.Bold;
            }
            else
            {
                p2Text.Foreground = Brushes.Gray;
            }

            Grid.SetRow(p2Text, 1);
            Grid.SetColumn(p2Text, 0);
            Grid.SetRow(p2Status, 1);
            Grid.SetColumn(p2Status, 1);
            grid.Children.Add(p2Text);
            grid.Children.Add(p2Status);

            matchBorder.Child = grid;

            // Position on canvas
            Canvas.SetLeft(matchBorder, x);
            Canvas.SetTop(matchBorder, top);

            BracketCanvas.Children.Add(matchBorder);
        }
    }

    private string ShortenName(string fullName)
    {
        if (fullName == "(BYE)") return fullName;
        int hyphenIndex = fullName.IndexOf('-');
        if (hyphenIndex >= 0 && hyphenIndex < fullName.Length - 1)
        {
            return fullName.Substring(hyphenIndex + 1).Trim();
        }
        return fullName.Length > 16 ? fullName.Substring(0, 14) + ".." : fullName;
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
