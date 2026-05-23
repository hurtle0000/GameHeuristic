using System;
using GameHeuristic.Core;

namespace GameHeuristic.Core.Submissions;

public class RogerRandom : IHeuristic
{
    public string Name => "Roger Random";
    private Random _random = new Random();

    public double Evaluate(Player[,] board, Player player)
    {
        // Just return a random value. Minimax will pick the column that happens to get the highest random number.
        return _random.NextDouble();
    }
}

public class LawrenceLow : IHeuristic
{
    public string Name => "Lawrence Low";

    public double Evaluate(Player[,] board, Player player)
    {
        double score = 0;
        Player opponent = player == Player.Red ? Player.Yellow : Player.Red;

        for (int r = 0; r < Board.Rows; r++)
        {
            for (int c = 0; c < Board.Columns; c++)
            {
                if (board[r, c] == player)
                {
                    // Favor lower column indices by giving them higher weight
                    score += (Board.Columns - c);
                }
                else if (board[r, c] == opponent)
                {
                    score -= (Board.Columns - c);
                }
            }
        }

        return score;
    }
}

public class DianaDiagonal : IHeuristic
{
    public string Name => "Diana Diagonal";

    public double Evaluate(Player[,] board, Player player)
    {
        double score = 0;
        Player opponent = player == Player.Red ? Player.Yellow : Player.Red;

        // Weights for different directions
        const double DiagonalWeight = 3.0;
        const double HorizontalWeight = 2.0;
        const double VerticalWeight = 1.0;

        // We'll look at every possible 4-slot window and score it
        
        // Horizontal
        for (int r = 0; r < Board.Rows; r++)
        {
            for (int c = 0; c <= Board.Columns - 4; c++)
            {
                score += EvaluateWindow(
                    board[r, c], board[r, c + 1], board[r, c + 2], board[r, c + 3], 
                    player, opponent) * HorizontalWeight;
            }
        }

        // Vertical
        for (int c = 0; c < Board.Columns; c++)
        {
            for (int r = 0; r <= Board.Rows - 4; r++)
            {
                score += EvaluateWindow(
                    board[r, c], board[r + 1, c], board[r + 2, c], board[r + 3, c], 
                    player, opponent) * VerticalWeight;
            }
        }

        // Diagonal (Down-Right)
        for (int r = 0; r <= Board.Rows - 4; r++)
        {
            for (int c = 0; c <= Board.Columns - 4; c++)
            {
                score += EvaluateWindow(
                    board[r, c], board[r + 1, c + 1], board[r + 2, c + 2], board[r + 3, c + 3], 
                    player, opponent) * DiagonalWeight;
            }
        }

        // Diagonal (Up-Right)
        for (int r = 3; r < Board.Rows; r++)
        {
            for (int c = 0; c <= Board.Columns - 4; c++)
            {
                score += EvaluateWindow(
                    board[r, c], board[r - 1, c + 1], board[r - 2, c + 2], board[r - 3, c + 3], 
                    player, opponent) * DiagonalWeight;
            }
        }

        return score;
    }

    private double EvaluateWindow(Player p1, Player p2, Player p3, Player p4, Player player, Player opponent)
    {
        int playerCount = 0;
        int opponentCount = 0;
        int emptyCount = 0;

        Player[] window = { p1, p2, p3, p4 };
        foreach (Player p in window)
        {
            if (p == player) playerCount++;
            else if (p == opponent) opponentCount++;
            else emptyCount++;
        }

        if (playerCount == 4) return 10000;
        if (playerCount == 3 && emptyCount == 1) return 100;
        if (playerCount == 2 && emptyCount == 2) return 10;
        
        if (opponentCount == 3 && emptyCount == 1) return -80; // Block opponent
        
        return 0;
    }
}
