 using System;
using GameHeuristic.Core;

namespace GameHeuristic.Core.Submissions.Y2026;

public class NicheAlgorithm : IHeuristic
{
    public string Name => "Student 2026 - Greedy Builder";

    public double Evaluate(Player[,] board, Player player)
    {
        return 10.0;
    }

}
