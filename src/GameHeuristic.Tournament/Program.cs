using System;
using System.Collections.Generic;
using System.Linq;
using GameHeuristic.Core;

namespace GameHeuristic.Tournament;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== Connect 4 Heuristic Tournament ===");
        
        string group = "All";
        for (int i = 0; i < args.Length; i++)
        {
            if ((args[i] == "--group" || args[i] == "-g") && i + 1 < args.Length)
            {
                group = args[i + 1];
                i++;
            }
        }

        List<string> availableGroups = HeuristicLoader.GetAvailableGroups();
        Console.WriteLine($"Available Groups: All, {string.Join(", ", availableGroups)}");
        Console.WriteLine($"Running tournament for group: {group}");

        List<IHeuristic> heuristics = HeuristicLoader.LoadHeuristics(group);

        if (heuristics.Count < 2)
        {
            Console.WriteLine($"Need at least 2 heuristics in group '{group}' to run a tournament.");
            return;
        }

        Console.WriteLine($"Found {heuristics.Count} heuristics for group '{group}'.");
        Dictionary<string, Stats> stats = heuristics.ToDictionary(h => h.Name, h => new Stats());

        // Round Robin: Each plays every other twice (once as Red, once as Yellow)
        for (int i = 0; i < heuristics.Count; i++)
        {
            for (int j = 0; j < heuristics.Count; j++)
            {
                if (i == j) continue;

                IHeuristic h1 = heuristics[i];
                IHeuristic h2 = heuristics[j];

                Console.Write($"Match: {h1.Name} (Red) vs {h2.Name} (Yellow) ... ");
                Player winner = PlayGame(h1, h2);
                
                if (winner == Player.Red)
                {
                    stats[h1.Name].Wins++;
                    stats[h2.Name].Losses++;
                    Console.WriteLine($"{h1.Name} wins!");
                }
                else if (winner == Player.Yellow)
                {
                    stats[h2.Name].Wins++;
                    stats[h1.Name].Losses++;
                    Console.WriteLine($"{h2.Name} wins!");
                }
                else
                {
                    stats[h1.Name].Draws++;
                    stats[h2.Name].Draws++;
                    Console.WriteLine("Draw!");
                }
            }
        }

        PrintLeaderboard(stats);
    }

    static Player PlayGame(IHeuristic h1, IHeuristic h2)
    {
        Board board = new Board();
        MinimaxAI ai1 = new MinimaxAI(h1, depth: 4); // Lower depth for faster tournament
        MinimaxAI ai2 = new MinimaxAI(h2, depth: 4);
        Player currentPlayer = Player.Red;

        while (true)
        {
            int move = (currentPlayer == Player.Red ? ai1 : ai2).GetBestMove(board, currentPlayer);
            if (move == -1 || !board.MakeMove(move, currentPlayer))
            {
                return currentPlayer == Player.Red ? Player.Yellow : Player.Red; // Forfeit
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

    static void PrintLeaderboard(Dictionary<string, Stats> stats)
    {
        Console.WriteLine("\n=== Leaderboard ===");
        Console.WriteLine($"{"Name",-25} | {"Wins",-5} | {"Losses",-6} | {"Draws",-5} | {"Score",-5}");
        Console.WriteLine(new string('-', 60));

        IOrderedEnumerable<KeyValuePair<string, Stats>> sorted = stats.OrderByDescending(s => s.Value.Score).ThenByDescending(s => s.Value.Wins);

        foreach (KeyValuePair<string, Stats> entry in sorted)
        {
            Console.WriteLine($"{entry.Key,-25} | {entry.Value.Wins,-5} | {entry.Value.Losses,-6} | {entry.Value.Draws,-5} | {entry.Value.Score,-5}");
        }
    }
}

class Stats
{
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int Draws { get; set; }
    public double Score => Wins + (Draws * 0.5);
}
