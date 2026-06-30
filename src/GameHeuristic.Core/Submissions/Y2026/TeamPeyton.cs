using System;
using GameHeuristic.Core;

namespace GameHeuristic.Core.Submissions.Y2026;


public class TeamPeyton : IHeuristic
{

    public string Name => "pey pey's algorithm";


    public double Evaluate(Player[,] board, Player player)
    {
        double score = 0;

        for (int r = 0; r < Board.Rows; r++)
        {
            for (int c = 0; c <= Board.Columns - 4; c++)
            {
                score += EvaluateWindow(
                    board[r, c], board[r, c + 1], board[r, c + 2], board[r, c + 3],
                    player);
            }
        }

        for (int c = 0; c < Board.Columns; c++)
        {
            for (int r = 0; r <= Board.Rows - 4; r++)
            {
                score += EvaluateWindow(
                    board[r, c], board[r + 1, c], board[r + 2, c], board[r + 3, c],
                    player);
            }
        }

        for (int r = 0; r <= Board.Rows - 4; r++)
        {
            for (int c = 0; c <= Board.Columns - 4; c++)
            {
                score += EvaluateWindow(
                    board[r, c], board[r + 1, c + 1], board[r + 2, c + 2], board[r + 3, c + 3],
                    player);
            }
        }

        for (int r = 3; r < Board.Rows; r++)
        {
            for (int c = 0; c <= Board.Columns - 4; c++)
            {
                score += EvaluateWindow(
                    board[r, c], board[r - 1, c + 1], board[r - 2, c + 2], board[r - 3, c + 3],
                    player);
            }
        }


        return score;
    }

    private double EvaluateWindow(Player p1, Player p2, Player p3, Player p4, Player player)
    {
        int playerCount = 0;
        int opponentCount = 0;
        int emptyCount = 0;

        Player[] window = { p1, p2, p3, p4 };
        foreach (Player p in window)
        {
            if (p == player) playerCount++;
            else if (p == Player.None) emptyCount++;
            else opponentCount++;
        }

        if (playerCount == 2 && emptyCount == 2) return 8;
        if (playerCount == 3 && emptyCount == 1) return 10; 

        return 0;
    }
}
