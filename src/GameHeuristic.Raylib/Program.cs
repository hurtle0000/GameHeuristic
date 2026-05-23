using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Raylib_cs;
using GameHeuristic.Core;

namespace GameHeuristic.RaylibUI;

class Program
{
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

        // 3. Set up 3D Perspective Camera
        Camera3D camera = new Camera3D
        {
            Position = new Vector3(0.0f, 1.5f, 9.5f), // Looking slightly down and directly in front
            Target = new Vector3(0.0f, -0.5f, 0.0f),  // Center of our board grid
            Up = new Vector3(0.0f, 1.0f, 0.0f),
            FovY = 45.0f,
            Projection = CameraProjection.Perspective
        };

        // 4. Timer Variables (Event Loop Tick)
        double lastTurnTime = Raylib.GetTime();
        double turnDelay = 0.8; // 800ms delay between AI turns

        // Main 3D Rendering & Game Clock Loop
        while (!Raylib.WindowShouldClose())
        {
            double currentTime = Raylib.GetTime();

            // 5. Game Clock Trigger (Timer Tick)
            if (gameState == GameState.Ongoing && (currentTime - lastTurnTime >= turnDelay))
            {
                int move = (currentPlayer == Player.Red ? ai1 : ai2).GetBestMove(board, currentPlayer);

                if (move == -1 || !board.MakeMove(move, currentPlayer))
                {
                    gameState = currentPlayer == Player.Red ? GameState.YellowWin : GameState.RedWin;
                }
                else
                {
                    gameState = board.CheckGameState();
                    // Switch turns
                    currentPlayer = currentPlayer == Player.Red ? Player.Yellow : Player.Red;
                }

                lastTurnTime = currentTime;
            }

            // Keyboard Spacebar input to manually reset the match
            if (Raylib.IsKeyPressed(KeyboardKey.Space))
            {
                board = new Board();
                currentPlayer = Player.Red;
                gameState = GameState.Ongoing;
                lastTurnTime = Raylib.GetTime();
            }

            // 6. Begin 3D Graphics Drawing
            Raylib.BeginDrawing();
            Raylib.ClearBackground(new Color(25, 30, 45, 255)); // Cool dark navy slate

            Raylib.BeginMode3D(camera);

            // Draw Blue Connect 4 Board Stand/Rack
            // Positioned at X=0, Y=-0.5, Z=0. Dimensions: width=7.6, height=6.6, depth=0.4
            Raylib.DrawCube(new Vector3(0.0f, -0.5f, 0.0f), 7.6f, 6.6f, 0.4f, Color.Blue);
            Raylib.DrawCubeWires(new Vector3(0.0f, -0.5f, 0.0f), 7.6f, 6.6f, 0.4f, Color.DarkBlue);

            // Draw Stand Feet
            Raylib.DrawCube(new Vector3(-3.5f, -3.9f, 0.0f), 0.5f, 0.4f, 2.0f, Color.Blue);
            Raylib.DrawCube(new Vector3(3.5f, -3.9f, 0.0f), 0.5f, 0.4f, 2.0f, Color.Blue);

            // Draw the Connect 4 grid pieces
            for (int r = 0; r < Board.Rows; r++)
            {
                for (int c = 0; c < Board.Columns; c++)
                {
                    // Map 2D array row/col [0,0] top-left to [5,6] bottom-right into 3D Cartesian coordinates
                    float x = (c - 3.0f) * 1.0f; // Scale columns horizontally
                    float y = (2.5f - r) * 1.0f; // Scale rows vertically (gravity falls downwards)

                    Player piece = board.GetPiece(r, c);
                    Vector3 position = new Vector3(x, y, 0.0f);

                    if (piece == Player.Red)
                    {
                        Raylib.DrawSphere(position, 0.4f, Color.Red);
                        Raylib.DrawSphereWires(position, 0.4f, 16, 16, Color.Maroon);
                    }
                    else if (piece == Player.Yellow)
                    {
                        Raylib.DrawSphere(position, 0.4f, Color.Yellow);
                        Raylib.DrawSphereWires(position, 0.4f, 16, 16, Color.Gold);
                    }
                    else
                    {
                        // Draw empty gray hollow circular slots inside the blue rack
                        Raylib.DrawSphere(position, 0.38f, new Color(50, 50, 75, 255));
                    }
                }
            }

            Raylib.EndMode3D();

            // 7. 2D HUD/Text Overlays
            Raylib.DrawText("3D CONNECT 4 VISUAL MATCH (RAYLIB)", 20, 20, 24, Color.White);
            Raylib.DrawText($"Red: {h1.Name}", 20, 60, 18, Color.Red);
            Raylib.DrawText($"Yellow: {h2.Name}", 20, 95, 18, Color.Yellow);

            // Game State HUD Label
            if (gameState == GameState.Ongoing)
            {
                string turnText = currentPlayer == Player.Red ? "RED AI Turn..." : "YELLOW AI Turn...";
                Color turnColor = currentPlayer == Player.Red ? Color.Red : Color.Yellow;
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
}
