using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GameHeuristic.Core;

namespace GameHeuristic.Terminal;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=======================================");
        Console.WriteLine("   Connect 4: Retro Console TUI UI");
        Console.WriteLine("=======================================");

        // 1. Dynamic Heuristic Loading (Reflection)
        List<IHeuristic> heuristics = HeuristicLoader.LoadHeuristics();

        Console.WriteLine("\nAvailable Heuristics:");
        Console.WriteLine("[0] Human Player");
        for (int i = 0; i < heuristics.Count; i++)
        {
            Console.WriteLine($"[{i + 1}] {heuristics[i].Name}");
        }

        // 2. Player Selection
        int p1Index = PromptSelection("Select Player 1 (Red) [0-" + heuristics.Count + "]: ", 0, heuristics.Count);
        int p2Index = PromptSelection("Select Player 2 (Yellow) [0-" + heuristics.Count + "]: ", 0, heuristics.Count);

        // Define players (Null means human)
        IHeuristic? h1 = p1Index == 0 ? null : heuristics[p1Index - 1];
        IHeuristic? h2 = p2Index == 0 ? null : heuristics[p2Index - 1];

        MinimaxAI? ai1 = h1 != null ? new MinimaxAI(h1, depth: 6) : null;
        MinimaxAI? ai2 = h2 != null ? new MinimaxAI(h2, depth: 6) : null;

        // 3. Main Game Loop Setup
        Board board = new Board();
        Player currentPlayer = Player.Red;

        Console.Clear();
        DrawBoard(board);

        while (true)
        {
            Console.WriteLine($"\n--- {(currentPlayer == Player.Red ? "Red" : "Yellow")}'s Turn ---");

            int chosenCol = -1;
            bool isHuman = (currentPlayer == Player.Red && ai1 == null) || (currentPlayer == Player.Yellow && ai2 == null);

            if (isHuman)
            {
                // Human Input Prompt
                while (true)
                {
                    Console.Write("Enter Column (0-6) to drop a piece: ");
                    string? input = Console.ReadLine();
                    if (int.TryParse(input, out int col) && col >= 0 && col < Board.Columns)
                    {
                        if (board.CanMakeMove(col))
                        {
                            chosenCol = col;
                            break;
                        }
                        Console.WriteLine("Column is full! Choose another column.");
                    }
                    else
                    {
                        Console.WriteLine("Invalid column! Enter a number between 0 and 6.");
                    }
                }
            }
            else
            {
                // AI search execution
                Console.WriteLine("AI is thinking...");
                MinimaxAI activeAI = currentPlayer == Player.Red ? ai1! : ai2!;
                chosenCol = activeAI.GetBestMove(board, currentPlayer);
                Thread.Sleep(500); // 500ms delay to make it readable
            }

            if (chosenCol == -1 || !board.MakeMove(chosenCol, currentPlayer))
            {
                Console.WriteLine($"{(currentPlayer == Player.Red ? "Red" : "Yellow")} forfeited the game!");
                break;
            }

            Console.Clear();
            DrawBoard(board);

            // Check game state boundaries
            GameState state = board.CheckGameState();
            if (state != GameState.Ongoing)
            {
                if (state == GameState.Draw)
                    Console.WriteLine("\n=== GAME OVER: It's a DRAW! ===");
                else
                    Console.WriteLine($"\n=== GAME OVER: {(state == GameState.RedWin ? "Red" : "Yellow")} WINS! ===");
                break;
            }

            // Swap players
            currentPlayer = currentPlayer == Player.Red ? Player.Yellow : Player.Red;
        }

        Console.WriteLine("\nPress any key to exit.");
        Console.ReadKey();
    }

    /// <summary>
    /// Procedurally prints the grid to console using ASCII symbols
    /// </summary>
    static void DrawBoard(Board board)
    {
        Console.WriteLine("  0   1   2   3   4   5   6  ");
        Console.WriteLine("┌───┬───┬───┬───┬───┬───┬───┐");
        for (int r = 0; r < Board.Rows; r++)
        {
            Console.Write("│");
            for (int c = 0; c < Board.Columns; c++)
            {
                Player piece = board.GetPiece(r, c);
                string symbol = piece switch
                {
                    Player.Red => " R ",
                    Player.Yellow => " Y ",
                    _ => "   "
                };

                // Add colors for standard terminals
                if (piece == Player.Red)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(symbol);
                    Console.ResetColor();
                }
                else if (piece == Player.Yellow)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write(symbol);
                    Console.ResetColor();
                }
                else
                {
                    Console.Write(symbol);
                }
                Console.Write("│");
            }
            Console.WriteLine();
            if (r < Board.Rows - 1)
                Console.WriteLine("├───┼───┼───┼───┼───┼───┼───┤");
        }
        Console.WriteLine("└───┴───┴───┴───┴───┴───┴───┘");
    }

    static int PromptSelection(string prompt, int min, int max)
    {
        while (true)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine();
            if (int.TryParse(input, out int result) && result >= min && result <= max)
            {
                return result;
            }
            Console.WriteLine($"Please enter a valid choice between {min} and {max}.");
        }
    }
}
