using System;
using GameHeuristic.Core;

namespace GameHeuristic.Core.Submissions.Baselines;

// A couple of example player heuristics for reference


// GCE test
public class GCE2026 : IHeuristic
{
    public string Name { get; set; } = "GCE2026";
    private Random _random = new Random();

    public double Evaluate(Player[,] board, Player player)
    {
        return 10.0d;
    }
}