using System;
using GameHeuristic.Core;

namespace GameHeuristic.Core.Submissions.Baselines;

// A couple of example player heuristics for reference


// GCE test
public class JoshuaKarlBot : IHeuristic
{
    public string Name { get; set; } = "JoshuaKarlBot";
    private Random _random = new Random();

    public double Evaluate(Player[,] board, Player player)
    {
        double score = 0;





        return score;
    }
}