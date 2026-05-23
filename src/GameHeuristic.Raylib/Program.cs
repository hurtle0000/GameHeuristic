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
        
        // Grab two default bots for the demo (Expert vs Student 2026)
        IHeuristic h1 = heuristics.FirstOrDefault(h => h.Name.Contains("Teacher")) ?? heuristics[0];
        IHeuristic h2 = heuristics.FirstOrDefault(h => h.Name.Contains("Student")) ?? heuristics[Math.Min(1, heuristics.Count - 1)];

        MinimaxAI ai1 = new MinimaxAI(h1, depth: 6);
        MinimaxAI ai2 = new MinimaxAI(h2, depth: 6);

        // 2. Initialize Core Board
        Board board = new Board();
        Player currentPlayer = Player.Red;
        GameState gameState = GameState.Ongoing;

        // Tracks the pieces already committed and settled on the board
        Player[,] settledGrid = new Player[Board.Rows, Board.Columns];

        // Currently active falling piece animation
        FallingPiece? activeAnimation = null;

        // 3. Set up 3D Perspective Camera
        Camera3D camera = new Camera3D
        {
            Position = new Vector3(0.0f, 1.8f, 9.5f), // Looking slightly down
            Target = new Vector3(0.0f, -0.5f, 0.0f),  // Center of our board grid
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

            // 5. Physics Update Loop
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

            // 6. Turn Logic Trigger
            if (gameState == GameState.Ongoing && activeAnimation == null && (currentTime - lastTurnTime >= turnDelay))
            {
                MinimaxAI activeAI = currentPlayer == Player.Red ? ai1 : ai2;
                int move = activeAI.GetBestMove(board, currentPlayer);

                if (move == -1 || !board.CanMakeMove(move))
                {
                    // Forfeit
                    gameState = currentPlayer == Player.Red ? GameState.YellowWin : GameState.RedWin;
                }
                else
                {
                    // Determine which row it will land on in the Board grid
                    int landingRow = -1;
                    for (int r = Board.Rows - 1; r >= 0; r--)
                    {
                        if (board.GetPiece(r, move) == Player.None)
                        {
                            landingRow = r;
                            break;
                        }
                    }

                    // Apply the move to the core logical Board
                    board.MakeMove(move, currentPlayer);

                    // Launch the fall animation instead of instantly populating settledGrid
                    activeAnimation = new FallingPiece
                    {
                        Row = landingRow,
                        Col = move,
                        Color = currentPlayer,
                        CurrentY = StartY,
                        TargetY = 2.0f - landingRow * 1.0f, // Match 3D layout coordinates
                        Velocity = 0.0f
                    };

                    // Check if game is over
                    gameState = board.CheckGameState();

                    // Swap player turns
                    currentPlayer = currentPlayer == Player.Red ? Player.Yellow : Player.Red;
                }
            }

            // Keyboard Spacebar input to manually reset the match
            if (Raylib.IsKeyPressed(KeyboardKey.Space))
            {
                board = new Board();
                settledGrid = new Player[Board.Rows, Board.Columns];
                activeAnimation = null;
                currentPlayer = Player.Red;
                gameState = GameState.Ongoing;
                lastTurnTime = Raylib.GetTime();
            }

            // 7. Begin 3D Graphics Drawing
            Raylib.BeginDrawing();
            Raylib.ClearBackground(new Color(25, 30, 45, 255)); // Cool dark navy slate

            Raylib.BeginMode3D(camera);

            // ================= RACK MESH LATTICE STRUCTURE =================
            // Instead of a single solid block, we draw the rack procedurally using overlapping
            // vertical and horizontal beams to make it a true hollow mesh structure.
            Color rackColor = new Color(20, 60, 160, 255); // Rich deep blue
            Color rackWireColor = new Color(10, 30, 80, 255);

            // A. Draw 8 Vertical Pillars (separating the 7 columns)
            // Pillar spacing: cols are centered around X=0, from c=0..6 mapped to x=-3..3
            // So we place 8 pillars at x = -3.5, -2.5, -1.5, -0.5, 0.5, 1.5, 2.5, 3.5
            for (int i = 0; i <= 7; i++)
            {
                float x = -3.5f + i * 1.0f;
                // Positioned at X=x, Y=-0.5, Z=0. Dimensions: width=0.15, height=6.0, depth=0.4
                Raylib.DrawCube(new Vector3(x, -0.5f, 0.0f), 0.16f, 6.0f, 0.4f, rackColor);
                Raylib.DrawCubeWires(new Vector3(x, -0.5f, 0.0f), 0.16f, 6.0f, 0.4f, rackWireColor);
            }

            // B. Draw 7 Horizontal Beams (separating the 6 rows)
            // Beam spacing: rows are centered around Y=-0.5, from r=0..5 mapped to y=2..-3
            // So we place 7 beams at y = 2.5, 1.5, 0.5, -0.5, -1.5, -2.5, -3.5
            for (int i = 0; i <= 6; i++)
            {
                float y = 2.5f - i * 1.0f;
                // Positioned at X=0, Y=y, Z=0. Dimensions: width=7.2, height=0.15, depth=0.4
                Raylib.DrawCube(new Vector3(0.0f, y, 0.0f), 7.2f, 0.16f, 0.4f, rackColor);
                Raylib.DrawCubeWires(new Vector3(0.0f, y, 0.0f), 7.2f, 0.16f, 0.4f, rackWireColor);
            }

            // C. Draw Stand Feet
            Raylib.DrawCube(new Vector3(-3.5f, -3.7f, 0.0f), 0.5f, 0.4f, 2.0f, rackColor);
            Raylib.DrawCubeWires(new Vector3(-3.5f, -3.7f, 0.0f), 0.5f, 0.4f, 2.0f, rackWireColor);
            Raylib.DrawCube(new Vector3(3.5f, -3.7f, 0.0f), 0.5f, 0.4f, 2.0f, rackColor);
            Raylib.DrawCubeWires(new Vector3(3.5f, -3.7f, 0.0f), 0.5f, 0.4f, 2.0f, rackWireColor);

            // ================= 3D TOKENS (FLAT COINS/DISCS) =================
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
                        Draw3DDisc(new Vector3(x, y, 0.0f), piece);
                    }
                }
            }

            // B. Draw Active Falling Piece Animation
            if (activeAnimation != null)
            {
                float animX = (activeAnimation.Col - 3.0f) * 1.0f;
                Draw3DDisc(new Vector3(animX, activeAnimation.CurrentY, 0.0f), activeAnimation.Color);
            }

            Raylib.EndMode3D();

            // 8. 2D HUD/Text Overlays
            Raylib.DrawText("3D ARCADIAN CONNECT 4 (RAYLIB)", 20, 20, 24, Color.White);
            Raylib.DrawText($"Red: {h1.Name}", 20, 60, 18, Color.Red);
            Raylib.DrawText($"Yellow: {h2.Name}", 20, 95, 18, Color.Yellow);

            // Game State HUD Label
            if (gameState == GameState.Ongoing)
            {
                string turnText = currentPlayer == Player.Red ? "RED AI Turn..." : "YELLOW AI Turn...";
                Color turnColor = currentPlayer == Player.Red ? Color.Red : Color.Yellow;
                if (activeAnimation != null)
                {
                    turnText = "GRAVITY DROP ANIMATION...";
                    turnColor = Color.LightGray;
                }
                Raylib.DrawText(turnText, 20, screenHeight - 60, 22, turnColor);
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

            Raylib.DrawText("Press [SPACEBAR] to Restart", screenWidth - 300, 20, 16, Color.LightGray);
            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }

    /// <summary>
    /// Procedurally draws a flat 3D coin/disc facing the camera using DrawCylinderEx
    /// aligned parallel to the Z-axis.
    /// </summary>
    static void Draw3DDisc(Vector3 center, Player color)
    {
        Color discColor = color == Player.Red ? Color.Red : Color.Yellow;
        Color wireColor = color == Player.Red ? Color.Maroon : Color.Gold;

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
