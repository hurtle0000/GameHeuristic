using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Raylib_cs;
using GameHeuristic.Core;

namespace GameHeuristic.RaylibUI;

class Program
{
    // Physics and animation config
    const float Gravity = 35.0f; // Gravitational acceleration in units/s^2
    const float StartY = 3.5f;   // Starting Y coordinate above the rack

    class FallingPiece
    {
        public int Row { get; set; }
        public int Col { get; set; }
        public Player Color { get; set; }
        public float CurrentY { get; set; }
        public float TargetY { get; set; }
        public float Velocity { get; set; }
        public float Elasticity { get; set; } = 0.35f; // Bounce energy retention (35%)
        public bool IsFinished { get; set; }
    }

    static void Main(string[] args)
    {
        const int screenWidth = 1000;
        const int screenHeight = 750;

        Raylib.InitWindow(screenWidth, screenHeight, "Connect 4 Heuristic Framework - 3D Raylib UI");
        Raylib.SetTargetFPS(60);

        // 1. Dynamic Heuristic Loading (Reflection)
        List<IHeuristic> heuristics = HeuristicLoader.LoadHeuristics();

        // 2. Player Selection Settings (0 represents Human, 1..N represents the loaded bots)
        int p1Selection = 0; // Player 1 (Red) defaults to Human
        int p2Selection = 1; // Player 2 (Yellow) defaults to the first available bot (e.g. Expert)
        
        // Find default selections based on names if possible
        for (int i = 0; i < heuristics.Count; i++)
        {
            if (heuristics[i].Name.Contains("Teacher") || heuristics[i].Name.Contains("Expert"))
                p2Selection = i + 1;
        }

        // Initialize Core Board
        Board board = new Board();
        Player currentPlayer = Player.Red;
        GameState gameState = GameState.Ongoing;

        // Tracks the pieces already committed and settled on the board
        Player[,] settledGrid = new Player[Board.Rows, Board.Columns];

        // Currently active falling piece animation
        FallingPiece? activeAnimation = null;

        // Tracks the coordinates of the winning line to trigger flashing (ValueTuples from Core)
        List<(int Row, int Col)> winningLine = new List<(int Row, int Col)>();

        // Tracks current AI move calculation scores for display
        double[] currentAiScores = new double[Board.Columns];
        for (int i = 0; i < Board.Columns; i++) currentAiScores[i] = double.NaN;
        bool isAiThinking = false;
        int pendingAiMove = -2; // -2 means no move, -1 means forfeit, 0-6 is the calculated move

        // 3. Set up 3D Perspective Camera (Pulled back for a wider view)
        Camera3D camera = new Camera3D
        {
            Position = new Vector3(0.0f, 2.0f, 11.5f), // Pulled back from 9.5f to 11.5f
            Target = new Vector3(0.0f, -0.5f, 0.0f),   // Center of our board grid
            Up = new Vector3(0.0f, 1.0f, 0.0f),
            FovY = 45.0f,
            Projection = CameraProjection.Perspective
        };

        // 4. Timer Variables for Turn Delays
        double lastTurnTime = Raylib.GetTime();
        double turnDelay = 0.5; // 500ms delay between AI choices

        // Main 3D Rendering & Physics loop
        while (!Raylib.WindowShouldClose())
        {
            float dt = Raylib.GetFrameTime();
            double currentTime = Raylib.GetTime();

            // ================= DYNAMIC CONTROLS & INTERACTIVE SELECTIONS =================
            // Cycle Player 1 (Red) using Key '1'
            if (Raylib.IsKeyPressed(KeyboardKey.One))
            {
                p1Selection = (p1Selection + 1) % (heuristics.Count + 1);
                ResetMatch(ref board, ref settledGrid, ref activeAnimation, ref currentPlayer, ref gameState, ref lastTurnTime, ref winningLine, currentAiScores, ref isAiThinking, ref pendingAiMove);
            }

            // Cycle Player 2 (Yellow) using Key '2'
            if (Raylib.IsKeyPressed(KeyboardKey.Two))
            {
                p2Selection = (p2Selection + 1) % (heuristics.Count + 1);
                ResetMatch(ref board, ref settledGrid, ref activeAnimation, ref currentPlayer, ref gameState, ref lastTurnTime, ref winningLine, currentAiScores, ref isAiThinking, ref pendingAiMove);
            }

            // Manual spacebar match reset
            if (Raylib.IsKeyPressed(KeyboardKey.Space))
            {
                ResetMatch(ref board, ref settledGrid, ref activeAnimation, ref currentPlayer, ref gameState, ref lastTurnTime, ref winningLine, currentAiScores, ref isAiThinking, ref pendingAiMove);
            }

            // ================= PHYSICS ANIMATION UPDATE LOOP =================
            if (activeAnimation != null)
            {
                // Euler integration: apply gravity to velocity
                activeAnimation.Velocity += Gravity * dt;
                // Move position downward
                activeAnimation.CurrentY -= activeAnimation.Velocity * dt;

                // Check landing boundary
                if (activeAnimation.CurrentY <= activeAnimation.TargetY)
                {
                    if (activeAnimation.Velocity > 1.5f)
                    {
                        // Elastic bounce: reverse direction and scale down energy
                        activeAnimation.CurrentY = activeAnimation.TargetY;
                        activeAnimation.Velocity = -activeAnimation.Velocity * activeAnimation.Elasticity;
                    }
                    else
                    {
                        // Settle into final position
                        activeAnimation.CurrentY = activeAnimation.TargetY;
                        activeAnimation.IsFinished = true;
                    }
                }

                if (activeAnimation.IsFinished)
                {
                    // Commit to settled board and trigger next turn clock
                    settledGrid[activeAnimation.Row, activeAnimation.Col] = activeAnimation.Color;
                    activeAnimation = null;
                    lastTurnTime = Raylib.GetTime(); // Reset turn timer
                }
            }

            // ================= GAME TURN LOGIC =================
            if (gameState == GameState.Ongoing && activeAnimation == null)
            {
                bool isRedTurn = currentPlayer == Player.Red;
                int currentSelection = isRedTurn ? p1Selection : p2Selection;

                if (currentSelection == 0)
                {
                    // ---------------- HUMAN TURN CONTROLS ----------------
                    // Option A: Mouse Clicks
                    if (Raylib.IsMouseButtonPressed(MouseButton.Left))
                    {
                        Vector2 mousePos = Raylib.GetMousePosition();
                        float colWidth = 72.0f; // Approx column width in pixels on screen
                        float boardCenterX = screenWidth / 2.0f;
                        int col = (int)Math.Round((mousePos.X - boardCenterX) / colWidth) + 3;

                        if (col >= 0 && col < Board.Columns)
                        {
                            if (board.CanMakeMove(col))
                            {
                                MakeMoveAndAnimate(col, board, currentPlayer, ref activeAnimation, ref gameState, ref currentPlayer, ref winningLine);
                                // Clear scores on human turn
                                for (int i = 0; i < Board.Columns; i++) currentAiScores[i] = double.NaN;
                            }
                        }
                    }

                    // Option B: Keyboard keys '0'..'6'
                    for (int col = 0; col < Board.Columns; col++)
                    {
                        if (Raylib.IsKeyPressed((KeyboardKey)((int)KeyboardKey.Zero + col)))
                        {
                            if (board.CanMakeMove(col))
                            {
                                MakeMoveAndAnimate(col, board, currentPlayer, ref activeAnimation, ref gameState, ref currentPlayer, ref winningLine);
                                // Clear scores on human turn
                                for (int i = 0; i < Board.Columns; i++) currentAiScores[i] = double.NaN;
                            }
                        }
                    }
                }
                else
                {
                    // ---------------- AI TURN CONTROLS ----------------
                    if (pendingAiMove != -2)
                    {
                        int move = pendingAiMove;
                        pendingAiMove = -2; // Reset

                        if (move == -1 || !board.CanMakeMove(move))
                        {
                            // Forfeit
                            gameState = currentPlayer == Player.Red ? GameState.YellowWin : GameState.RedWin;
                        }
                        else
                        {
                            MakeMoveAndAnimate(move, board, currentPlayer, ref activeAnimation, ref gameState, ref currentPlayer, ref winningLine);
                        }
                    }
                    else if (!isAiThinking && currentTime - lastTurnTime >= turnDelay)
                    {
                        IHeuristic activeHeuristic = heuristics[currentSelection - 1];
                        MinimaxAI ai = new MinimaxAI(activeHeuristic, depth: 6);
                        
                        // Clear scores before starting search
                        for (int i = 0; i < Board.Columns; i++) currentAiScores[i] = double.NaN;
                        
                        isAiThinking = true;

                        // Subscribe to live column score updates
                        ai.OnColumnEvaluated = (col, score) =>
                        {
                            currentAiScores[col] = score;
                        };

                        Board startBoard = board;
                        Player startPlayer = currentPlayer;

                        // Run the AI search on a background task so Raylib loop stays perfectly smooth at 60 FPS
                        System.Threading.Tasks.Task.Run(() =>
                        {
                            int move = ai.GetBestMove(startBoard, startPlayer);
                            
                            // Only apply if state hasn't changed or been reset
                            if (board == startBoard && currentPlayer == startPlayer && gameState == GameState.Ongoing)
                            {
                                pendingAiMove = move;
                            }
                            isAiThinking = false;
                        });
                    }
                }
            }

            // ================= 3D GRAPHICS DRAWING =================
            Raylib.BeginDrawing();
            Raylib.ClearBackground(new Color(25, 30, 45, 255)); // Cool dark navy slate

            Raylib.BeginMode3D(camera);

            // ================= RACK MESH LATTICE STRUCTURE =================
            // Procedurally constructed rack using overlapping rectangular pillars/beams
            Color rackColor = new Color(20, 60, 160, 255); // Rich deep blue
            Color rackWireColor = new Color(10, 30, 80, 255);

            // A. Draw 8 Vertical Pillars (separating the 7 columns)
            for (int i = 0; i <= 7; i++)
            {
                float x = -3.5f + i * 1.0f;
                Raylib.DrawCube(new Vector3(x, -0.5f, 0.0f), 0.16f, 6.0f, 0.4f, rackColor);
                Raylib.DrawCubeWires(new Vector3(x, -0.5f, 0.0f), 0.16f, 6.0f, 0.4f, rackWireColor);
            }

            // B. Draw 7 Horizontal Beams (separating the 6 rows)
            for (int i = 0; i <= 6; i++)
            {
                float y = 2.5f - i * 1.0f;
                Raylib.DrawCube(new Vector3(0.0f, y, 0.0f), 7.2f, 0.16f, 0.4f, rackColor);
                Raylib.DrawCubeWires(new Vector3(0.0f, y, 0.0f), 7.2f, 0.16f, 0.4f, rackWireColor);
            }

            // C. Draw Stand Feet
            Raylib.DrawCube(new Vector3(-3.5f, -3.7f, 0.0f), 0.5f, 0.4f, 2.0f, rackColor);
            Raylib.DrawCubeWires(new Vector3(-3.5f, -3.7f, 0.0f), 0.5f, 0.4f, 2.0f, rackWireColor);
            Raylib.DrawCube(new Vector3(3.5f, -3.7f, 0.0f), 0.5f, 0.4f, 2.0f, rackColor);
            Raylib.DrawCubeWires(new Vector3(3.5f, -3.7f, 0.0f), 0.5f, 0.4f, 2.0f, rackWireColor);

            // ================= 3D FLAT TOKENS (COINS/DISCS) =================
            // A. Draw Settled Pieces on the Board
            for (int r = 0; r < Board.Rows; r++)
            {
                for (int c = 0; c < Board.Columns; c++)
                {
                    float x = (c - 3.0f) * 1.0f;
                    float y = (2.0f - r) * 1.0f;

                    Player piece = settledGrid[r, c];
                    if (piece != Player.None)
                    {
                        // Check if this token belongs to the winning sequence
                        bool isWinningToken = winningLine.Any(t => t.Row == r && t.Col == c);
                        // Flashes neon-white every 200ms
                        bool shouldFlash = isWinningToken && ((int)(Raylib.GetTime() * 5) % 2 == 0);

                        Draw3DDisc(new Vector3(x, y, 0.0f), piece, shouldFlash);
                    }
                }
            }

            // B. Draw Active Falling Piece Animation (Never flashes as it is active)
            if (activeAnimation != null)
            {
                float animX = (activeAnimation.Col - 3.0f) * 1.0f;
                Draw3DDisc(new Vector3(animX, activeAnimation.CurrentY, 0.0f), activeAnimation.Color, false);
            }

            Raylib.EndMode3D();

            // ================= 2D HOLOGRAPHIC FLOATING SCORES =================
            // Project each column's 3D coordinate just above the top of the rack to 2D screen space
            for (int c = 0; c < Board.Columns; c++)
            {
                double score = currentAiScores[c];
                if (!double.IsNaN(score))
                {
                    float x = (c - 3.0f) * 1.0f;
                    // Project 3D point above the column to 2D screen coordinates
                    Vector3 worldPos = new Vector3(x, 3.2f, 0.0f);
                    Vector2 screenPos = Raylib.GetWorldToScreen(worldPos, camera);

                    if (screenPos.X > 0 && screenPos.X < screenWidth && screenPos.Y > 0 && screenPos.Y < screenHeight)
                    {
                        string scoreText = score.ToString("F1");
                        Color textColor = Color.White;
                        Color panelBg = new Color(20, 20, 30, 200); // Glassmorphism backing

                        if (score > 900000)
                        {
                            scoreText = "WIN";
                            textColor = Color.Green;
                        }
                        else if (score < -900000)
                        {
                            scoreText = "LOSS";
                            textColor = Color.Red;
                        }
                        else if (score > 0)
                        {
                            textColor = Color.Green;
                        }
                        else if (score < 0)
                        {
                            textColor = Color.Red;
                        }

                        int fontSize = 14;
                        int textWidth = Raylib.MeasureText(scoreText, fontSize);
                        
                        // Draw a sleek rounded rectangle backing box
                        int rectWidth = textWidth + 14;
                        int rectHeight = fontSize + 10;
                        Rectangle rect = new Rectangle(screenPos.X - rectWidth / 2f, screenPos.Y - rectHeight / 2f, rectWidth, rectHeight);
                        Raylib.DrawRectangleRounded(rect, 0.4f, 4, panelBg);
                        Raylib.DrawRectangleRoundedLines(rect, 0.4f, 4, 1.0f, textColor);

                        // Draw text centered inside the box
                        Raylib.DrawText(scoreText, (int)(screenPos.X - textWidth / 2f), (int)(screenPos.Y - fontSize / 2f), fontSize, textColor);
                    }
                }
            }

            // ================= 2D HUD / TEXT OVERLAYS =================
            Raylib.DrawText("3D CONNECT 4 ARCADIAN STAGE", 20, 20, 24, Color.White);

            // Dynamic Player 1 Select Label
            string p1NameText = p1Selection == 0 ? "HUMAN PLAYER" : heuristics[p1Selection - 1].Name;
            Raylib.DrawText($"[1] Player 1 (Red): {p1NameText}", 20, 60, 18, Color.Red);

            // Dynamic Player 2 Select Label
            string p2NameText = p2Selection == 0 ? "HUMAN PLAYER" : heuristics[p2Selection - 1].Name;
            Raylib.DrawText($"[2] Player 2 (Yellow): {p2NameText}", 20, 95, 18, Color.Yellow);

            // Turn Indicator / Animation Label
            if (gameState == GameState.Ongoing)
            {
                string turnText = currentPlayer == Player.Red ? "RED Player Turn..." : "YELLOW Player Turn...";
                Color turnColor = currentPlayer == Player.Red ? Color.Red : Color.Yellow;
                
                if (activeAnimation != null)
                {
                    turnText = "GRAVITY DROP PHYSICS TICK...";
                    turnColor = Color.LightGray;
                }
                else if (isAiThinking)
                {
                    turnText = "AI MINIMAX CALCULATING WEIGHTS...";
                    turnColor = Color.SkyBlue;
                }
                else
                {
                    bool currentIsHuman = (currentPlayer == Player.Red && p1Selection == 0) || (currentPlayer == Player.Yellow && p2Selection == 0);
                    if (currentIsHuman)
                    {
                        turnText += " (Click Column or Press [0-6] to Play)";
                    }
                }
                Raylib.DrawText(turnText, 20, screenHeight - 60, 20, turnColor);
            }
            else
            {
                string endText = gameState switch
                {
                    GameState.RedWin => "RED WINS!",
                    GameState.YellowWin => "YELLOW WINS!",
                    _ => "DRAW GAME!"
                };
                Color endColor = gameState == GameState.RedWin ? Color.Red : (gameState == GameState.YellowWin ? Color.Yellow : Color.Gray);
                Raylib.DrawText(endText, 20, screenHeight - 60, 32, endColor);
            }

            Raylib.DrawText("Controls:", screenWidth - 280, 60, 16, Color.LightGray);
            Raylib.DrawText(" - Press [1] to cycle Red Player", screenWidth - 280, 85, 14, Color.Gray);
            Raylib.DrawText(" - Press [2] to cycle Yellow Player", screenWidth - 280, 105, 14, Color.Gray);
            Raylib.DrawText(" - Click Column or Press [0-6] to play", screenWidth - 280, 125, 14, Color.Gray);
            Raylib.DrawText(" - Press [SPACE] to Restart Match", screenWidth - 280, 145, 14, Color.Gray);

            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }

    /// <summary>
    /// Resets the game board and animation states
    /// </summary>
    static void ResetMatch(ref Board board, ref Player[,] settledGrid, ref FallingPiece? activeAnimation, ref Player currentPlayer, ref GameState gameState, ref double lastTurnTime, ref List<(int Row, int Col)> winningLine, double[] currentAiScores, ref bool isAiThinking, ref int pendingAiMove)
    {
        board = new Board();
        settledGrid = new Player[Board.Rows, Board.Columns];
        activeAnimation = null;
        currentPlayer = Player.Red;
        gameState = GameState.Ongoing;
        lastTurnTime = Raylib.GetTime();
        winningLine.Clear();
        for (int i = 0; i < Board.Columns; i++) currentAiScores[i] = double.NaN;
        isAiThinking = false;
        pendingAiMove = -2;
    }

    /// <summary>
    /// Commits a column choice, calculates landing row coordinates, and launches the fall animation.
    /// </summary>
    static void MakeMoveAndAnimate(int col, Board board, Player player, ref FallingPiece? activeAnimation, ref GameState gameState, ref Player nextPlayer, ref List<(int Row, int Col)> winningLine)
    {
        int landingRow = board.GetLandingRow(col);
        board.MakeMove(col, player);

        activeAnimation = new FallingPiece
        {
            Row = landingRow,
            Col = col,
            Color = player,
            CurrentY = StartY,
            TargetY = 2.0f - landingRow * 1.0f,
            Velocity = 0.0f
        };

        gameState = board.CheckGameState();
        if (gameState != GameState.Ongoing && (gameState == GameState.RedWin || gameState == GameState.YellowWin))
        {
            winningLine = board.GetWinningLine();
        }

        nextPlayer = player == Player.Red ? Player.Yellow : Player.Red;
    }

    /// <summary>
    /// Procedurally draws a flat 3D coin/disc facing the camera using DrawCylinderEx.
    /// If flashing is active, colors it in bright neon white.
    /// </summary>
    static void Draw3DDisc(Vector3 center, Player color, bool flash)
    {
        Color discColor;
        Color wireColor;

        if (flash)
        {
            discColor = Color.White;
            wireColor = Color.SkyBlue;
        }
        else
        {
            discColor = color == Player.Red ? Color.Red : Color.Yellow;
            wireColor = color == Player.Red ? Color.Maroon : Color.Gold;
        }

        // Position start/end points along the Z axis centered on the coordinate
        Vector3 startPoint = new Vector3(center.X, center.Y, -0.15f);
        Vector3 endPoint = new Vector3(center.X, center.Y, 0.15f);
        float radius = 0.4f;

        // Draw flat cylinder disc
        Raylib.DrawCylinderEx(startPoint, endPoint, radius, radius, 24, discColor);
        // Draw cylinder outlines/wires to give it a neat 3D beveled appearance
        Raylib.DrawCylinderWiresEx(startPoint, endPoint, radius, radius, 24, wireColor);
    }
}
