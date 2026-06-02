using System;
using GameHeuristic.Core;

namespace GameHeuristic.Core.Submissions.Baselines;

// A couple of example player heuristics for reference


// Roger Random evaluates every possible move as a different random number
public class RogerRandom : IHeuristic
{
    public string Name { get; set; } = "Roger Random";
    private Random _random = new Random();

    public double Evaluate(Player[,] board, Player player)
    {
        return _random.NextDouble();
    }
}

// Lawrence Low likes to keep their tokens as low as possible on the board
public class LawrenceLow : IHeuristic
{
    public string Name { get; set; } = "Lawrence Low";

    public double Evaluate(Player[,] board, Player player)
    {
        double score = 0;

        for (int r = 0; r < Board.Rows; r++)
        {
            // Remember, row 0 is the "top" row so has the worst score
            for (int c = 0; c < Board.Columns; c++)
            {
                if (board[r, c] == player)
                {
                    score += (r);
                }
            }
        }

        return score;
    }
}
