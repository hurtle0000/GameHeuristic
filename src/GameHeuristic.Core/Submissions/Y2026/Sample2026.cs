using System;
using GameHeuristic.Core;

namespace GameHeuristic.Core.Submissions.Y2026;


public class TeamPeyton : IHeuristic
{
    public string Name => "pey pey's algorithm";

   
    public double Evaluate(Player[,] board, Player player)
    {
        return 10.0;
    }
}
