using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GameHeuristic.Core;

namespace GameHeuristic.WinForms;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainWindow());
    }
}

public class MainWindow : Form
{
    // Core Game Fields
    private Board _board = new Board();
    private readonly Button[,] _uiCells = new Button[Board.Rows, Board.Columns];
    private List<IHeuristic> _heuristics = new List<IHeuristic>();

    // Controls
    private ComboBox _player1Combo = null!;
    private ComboBox _player2Combo = null!;
    private Button _startButton = null!;
    private Button _resetButton = null!;
    private Label _statusLabel = null!;
    private System.Windows.Forms.Timer _gameTimer = null!;

    // Search Engine & Turn Fields
    private MinimaxAI? _ai1;
    private MinimaxAI? _ai2;
    private Player _currentPlayer;

    public MainWindow()
    {
        Text = "Connect 4 Heuristic Framework - WinForms UI";
        Width = 900;
        Height = 650;
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;

        InitializeControls();
        InitializeBoard();
        LoadHeuristics();
    }

    /// <summary>
    /// Procedurally creates all buttons, dropdown combos, and labels
    /// </summary>
    private void InitializeControls()
    {
        // 1. Sidebar Control Panel
        Panel sidebar = new Panel
        {
            Dock = DockStyle.Right,
            Width = 250,
            BackColor = Color.FromArgb(240, 240, 240),
            Padding = new Padding(15)
        };
        Controls.Add(sidebar);

        Label title = new Label { Text = "Match Settings", Font = new Font("Segoe UI", 14, FontStyle.Bold), Height = 30, Dock = DockStyle.Top };
        sidebar.Controls.Add(title);

        Label p1Label = new Label { Text = "Player 1 (Red):", Height = 20, Dock = DockStyle.Top, Margin = new Padding(0, 10, 0, 0) };
        sidebar.Controls.Add(p1Label);

        _player1Combo = new ComboBox { Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList, Height = 25 };
        sidebar.Controls.Add(_player1Combo);

        Label p2Label = new Label { Text = "Player 2 (Yellow):", Height = 20, Dock = DockStyle.Top, Margin = new Padding(0, 15, 0, 0) };
        sidebar.Controls.Add(p2Label);

        _player2Combo = new ComboBox { Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList, Height = 25 };
        sidebar.Controls.Add(_player2Combo);

        _startButton = new Button { Text = "Start Match", Height = 40, Dock = DockStyle.Bottom, BackColor = Color.LightGreen, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
        _startButton.Click += OnStartClick;
        sidebar.Controls.Add(_startButton);

        _resetButton = new Button { Text = "Reset Board", Height = 30, Dock = DockStyle.Bottom };
        _resetButton.Click += OnResetClick;
        sidebar.Controls.Add(_resetButton);

        _statusLabel = new Label
        {
            Text = "Select players and start",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.DarkBlue,
            TextAlign = ContentAlignment.MiddleCenter,
            Height = 60,
            Dock = DockStyle.Bottom
        };
        sidebar.Controls.Add(_statusLabel);

        // 2. Main Game Panel (Grid)
        TableLayoutPanel gridPanel = new TableLayoutPanel
        {
            RowCount = Board.Rows,
            ColumnCount = Board.Columns,
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(43, 87, 154), // Core Blue theme
            Padding = new Padding(15)
        };
        Controls.Add(gridPanel);

        // Adjust row/column styles to distribute space equally
        for (int c = 0; c < Board.Columns; c++)
            gridPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / Board.Columns));
        for (int r = 0; r < Board.Rows; r++)
            gridPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / Board.Rows));

        // 3. Populate grid buttons
        for (int r = 0; r < Board.Rows; r++)
        {
            for (int c = 0; c < Board.Columns; c++)
            {
                Button btn = new Button
                {
                    Dock = DockStyle.Fill,
                    Margin = new Padding(6),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.White,
                    Enabled = false // Disabled so users don't trigger click events directly on grid cells
                };
                
                // Set round circle border regions (creates neat round Connect 4 tokens in WinForms)
                btn.Paint += (s, e) =>
                {
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                    {
                        path.AddEllipse(btn.ClientRectangle);
                        btn.Region = new Region(path);
                    }
                };

                gridPanel.Controls.Add(btn, c, r);
                _uiCells[r, c] = btn;
            }
        }

        // 4. Initialize Game Timer
        _gameTimer = new System.Windows.Forms.Timer();
        _gameTimer.Interval = 500; // 500ms delay between AI turns
        _gameTimer.Tick += GameTimer_Tick;
    }

    private void InitializeBoard()
    {
        _board = new Board();
        UpdateBoardUI();
    }

    private void LoadHeuristics()
    {
        // Dynamic loading from shared project
        _heuristics = HeuristicLoader.LoadHeuristics();

        List<string> botNames = _heuristics.Select(h => h.Name).ToList();

        _player1Combo.Items.AddRange(botNames.ToArray());
        _player2Combo.Items.AddRange(botNames.ToArray());

        if (botNames.Count > 0)
        {
            _player1Combo.SelectedIndex = 0;
            _player2Combo.SelectedIndex = Math.Min(1, botNames.Count - 1);
        }
    }

    #region Single Match Timer Loop (A-Level Standard)

    private void OnStartClick(object? sender, EventArgs e)
    {
        if (_gameTimer.Enabled) return;

        InitializeBoard();
        _startButton.Enabled = false;

        IHeuristic h1 = _heuristics[_player1Combo.SelectedIndex];
        IHeuristic h2 = _heuristics[_player2Combo.SelectedIndex];

        _ai1 = new MinimaxAI(h1, depth: 6);
        _ai2 = new MinimaxAI(h2, depth: 6);
        _currentPlayer = Player.Red;

        _statusLabel.Text = "Red's turn...";

        // Start event-driven loop
        _gameTimer.Start();
    }

    private void GameTimer_Tick(object? sender, EventArgs e)
    {
        _gameTimer.Stop(); // Temporarily stop to prevent execution overlap

        // Compute AI move on the UI thread sequentially (Timer ticks yield CPU, keeping UI alive!)
        int move = (_currentPlayer == Player.Red ? _ai1! : _ai2!).GetBestMove(_board, _currentPlayer);

        if (move == -1 || !_board.MakeMove(move, _currentPlayer))
        {
            _statusLabel.Text = $"{(_currentPlayer == Player.Red ? "Red" : "Yellow")} forfeited!";
            CleanupGame();
            return;
        }

        UpdateBoardUI();

        GameState state = _board.CheckGameState();
        if (state != GameState.Ongoing)
        {
            if (state == GameState.Draw)
                _statusLabel.Text = "Draw Game!";
            else
                _statusLabel.Text = $"{(_currentPlayer == Player.Red ? "Red" : "Yellow")} Wins!";
            CleanupGame();
            return;
        }

        // Swap players
        _currentPlayer = _currentPlayer == Player.Red ? Player.Yellow : Player.Red;
        _statusLabel.Text = $"{(_currentPlayer == Player.Red ? "Red" : "Yellow")}'s turn...";

        _gameTimer.Start(); // Re-enable for the next move tick
    }

    private void CleanupGame()
    {
        _gameTimer.Stop();
        _startButton.Enabled = true;
    }

    private void OnResetClick(object? sender, EventArgs e)
    {
        CleanupGame();
        InitializeBoard();
        _statusLabel.Text = "Select players and start";
    }

    /// <summary>
    /// Simply updates the background color of round buttons based on array state
    /// </summary>
    private void UpdateBoardUI()
    {
        for (int r = 0; r < Board.Rows; r++)
        {
            for (int c = 0; c < Board.Columns; c++)
            {
                Player piece = _board.GetPiece(r, c);
                _uiCells[r, c].BackColor = piece switch
                {
                    Player.Red => Color.Red,
                    Player.Yellow => Color.Yellow,
                    _ => Color.White
                };
            }
        }
    }

    #endregion
}
