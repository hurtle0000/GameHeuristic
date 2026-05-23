using System;
using GameHeuristic.Core;

namespace GameHeuristic.Core.Submissions.Y2026;

/// <summary>
/// A sample student submission for the class of 2026.
/// This bot greedily tries to build its own lines of 2 and 3, completely ignoring the opponent's moves.
/// </summary>
public class StudentHeuristic2026 : IHeuristic
{
    public string Name => "Student 2026 - Greedy Builder";

    public double Evaluate(Player[,] board, Player player)
    {
        double score = 0;

        // Evaluate all possible 4-slot windows (horizontal, vertical, diagonal)
        
        // Horizontal
        for (int r = 0; r < Board.Rows; r++)
        {
            for (int c = 0; c <= Board.Columns - 4; c++)
            {
                score += EvaluateWindow(
                    board[r, c], board[r, c + 1], board[r, c + 2], board[r, c + 3], 
                    player);
            }
        }

        // Vertical
        for (int c = 0; c < Board.Columns; c++)
        {
            for (int r = 0; r <= Board.Rows - 4; r++)
            {
                score += EvaluateWindow(
                    board[r, c], board[r + 1, c], board[r + 2, c], board[r + 3, c], 
                    player);
            }
        }

        // Diagonal (Down-Right)
        for (int r = 0; r <= Board.Rows - 4; r++)
        {
            for (int c = 0; c <= Board.Columns - 4; c++)
            {
                score += EvaluateWindow(
                    board[r, c], board[r + 1, c + 1], board[r + 2, c + 2], board[r + 3, c + 3], 
                    player);
            }
        }

        // Diagonal (Up-Right)
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

        // If the opponent has a piece in this window, we can't make a 4-in-a-row here.
        if (opponentCount > 0) return 0;

        if (playerCount == 4) return 10000;          // Win
        if (playerCount == 3 && emptyCount == 1) return 100; // Prioritize lines of 3
        if (playerCount == 2 && emptyCount == 2) return 10;  // Prioritize lines of 2
        
        return 0;
    }
}
