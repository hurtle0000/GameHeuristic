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

public class daynah : IHeuristic
{
    public string Name { get; set; } = "daynah";
    private Random _random = new Random();


    public double Evaluate(Player[,] board, Player player)
    {

        double score = 0.0d;

        for (int i = 0; i < 5; i++)
        {
            if (board[3, i] == player)
            {
                if (1 < i && i < 4)
                {
                    if (board[3, i + 1] == player & board[3, i - 1] == player)
                    {
                        score += 10.0d;
                    }
                    else if (board[3, i + 2] == player & board[3, i - 2] == player)
                    {
                        score += 2.0d;
                    }

                }

                score += 10.0d;
            }
            else if (board[2, i] == player || board[4, i] == player)
            {
                if (1 < i && i < 4)
                {
                    if (board[2, i + 1] == player & board[2, i - 1] == player || board[4, i + 1] == player & board[4, i - 1] == player)
                    {
                        score += 10.0d;
                    }
                    else if (board[2, i + 1] == player & board[2, i - 1] == player || board[4, i + 1] == player & board[4, i - 1] == player)
                    {
                        score += 2.0d;
                    }

                }

                score += 7.0d;
            }
            else if (board[1, i] == player || board[5, i] == player)
            {
                if (1 < i && i < 4)
                {
                    if (board[1, i + 1] == player & board[1, i - 1] == player || board[5, i + 1] == player & board[5, i - 1] == player)
                    {
                        score += 10.0d;
                    }
                    else if (board[1, i + 1] == player & board[1, i - 1] == player || board[5, i + 1] == player & board[5, i - 1] == player)
                    {
                        score += 2.0d;
                    }

                }

                score += 3.0d;
            }
            else
            {
                score += 1.0d;
            }

        }


        return score;
    }
}
