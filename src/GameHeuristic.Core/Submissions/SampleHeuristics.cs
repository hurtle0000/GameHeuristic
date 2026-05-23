using System;

namespace GameHeuristic.Core.Submissions;

public class RandomHeuristic : IHeuristic
{
    public string Name => "Random Bot";
    private readonly Random _random = new Random();

    public double Evaluate(Player[,] board, Player player)
    {
        return _random.NextDouble();
    }
}

public class CenterPreferenceHeuristic : IHeuristic
{
    public string Name => "Center Preference Bot";

    public double Evaluate(Player[,] board, Player player)
    {
        double score = 0;
        int centerCol = Board.Columns / 2;

        for (int r = 0; r < Board.Rows; r++)
        {
            if (board[r, centerCol] == player)
                score += 3;
            else if (board[r, centerCol] != Player.None)
                score -= 3;
        }

        return score;
    }
}
