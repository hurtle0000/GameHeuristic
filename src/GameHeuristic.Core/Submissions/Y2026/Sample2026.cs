 using System;
using GameHeuristic.Core;

namespace GameHeuristic.Core.Submissions.Y2026;

/// <summary>
/// A sample student submission for the class of 2026.
/// This bot greedily tries to build its own lines of 2 completely ignoring the opponent's moves.
/// </summary>
public class StudentHeuristic2026 : IHeuristic
{
    public string Name => "James&JPDestroyer??";

    private static int[,] Weights = new int[Board.Rows, Board.Columns]
    {
        { 3, 4, 5, 7, 5, 4, 3 },
        { 4, 6, 8, 10, 8, 6, 4 },
        { 5, 8, 11, 13, 11, 8, 5 },
        { 5, 8, 11, 13, 11, 8, 5 },
        { 4, 6, 8, 10, 8, 6, 4 },
        { 3, 4, 5, 7, 5, 4, 3 }
    };

    /// <summary>
    /// This evaluation scores moves solely on building out rows of 2.  It doesn't look at the opponent.
    ///
    /// Look at each cell on the board that start a winning position and calculate a score for each horizontal,
    /// vertical and diagonal line that could start there.  If there is an opponent piece in the line, it
    /// can't be a winning position, so score that zero
    /// </summary>
    /// <param name="board"></param>
    /// <param name="player"></param>
    /// <returns></returns>
    public double Evaluate(Player[,] board, Player player)
    {
        double score = 0;

        //for (int r = 0; r < Board.Rows; r++)
        //{
        //    for (int c = 0; c <= Board.Columns - 4; c++)
        //    {
        //        score += EvaluateWindow(
        //            board[r, c], board[r, c + 1], board[r, c + 2], board[r, c + 3],
        //            player);
        //    }
        //}

        //for (int c = 0; c < Board.Columns; c++)
        //{
        //    for (int r = 0; r <= Board.Rows - 4; r++)
        //    {
        //        score += EvaluateWindow(
        //            board[r, c], board[r + 1, c], board[r + 2, c], board[r + 3, c],
        //            player);
        //    }
        //}

        //for (int r = 0; r <= Board.Rows - 4; r++)
        //{
        //    for (int c = 0; c <= Board.Columns - 4; c++)
        //    {
        //        score += EvaluateWindow(
        //            board[r, c], board[r + 1, c + 1], board[r + 2, c + 2], board[r + 3, c + 3],
        //            player);
        //    }
        //}

        //for (int r = 3; r < Board.Rows; r++)
        //{
        //    for (int c = 0; c <= Board.Columns - 4; c++)
        //    {
        //        score += EvaluateWindow(
        //            board[r, c], board[r - 1, c + 1], board[r - 2, c + 2], board[r - 3, c + 3],
        //            player);
        //    }
        //}

        // Evaluate all possible moves (columns only, since gravity decides row)
        for (int col = 0; col < Board.Columns; col++)
        {
            int row = GetDropRow(board, col);

            if (row == -1)
                continue; // column full

            score += Weights[row, col];
        }

        return score;
    }

    private int GetDropRow(Player[,] board, int col)
    {
        for (int r = Board.Rows - 1; r >= 0; r--)
        {
            if (board[r, col] == Player.None)
                return r;
        }

        return -1;
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

        if (playerCount == 2 && emptyCount == 2) return 10;  // Prioritize lines of 2

        return 0;
    }

    private bool IsWinningMove(Player[,] board, Player player)
    {
        // placeholder: replace with your existing win detection logic
        return false;
    }



}
