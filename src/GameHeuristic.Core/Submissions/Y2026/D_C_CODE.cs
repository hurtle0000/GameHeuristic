using System;
using GameHeuristic.Core;

namespace GameHeuristic.Core.Submissions.Baselines;

// A couple of example player heuristics for reference


// GCE test
public class D_C_CODE : IHeuristic
{
    public string Name { get; set; } = "Daniel / Charles submission";
    private Random _random = new Random();

    public double Evaluate(Player[,] board, Player player)
    {
        // test test test test
        return 10.0d;
    }
}