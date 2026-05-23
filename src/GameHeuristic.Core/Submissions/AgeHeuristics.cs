using System;
using GameHeuristic.Core;

namespace GameHeuristic.Core.Submissions;

/// <summary>
/// A 6-year-old just wants to see their colors on the board.
/// They don't have much strategy, but they like the middle and they like tall towers.
/// </summary>
public class SixYearOld : IHeuristic
{
    public string Name => "Six-Year-Old (Billy)";
    private Random _random = new Random();

    public double Evaluate(Player[,] board, Player player)
    {
        double score = 0;
        int centerCol = Board.Columns / 2;

        for (int r = 0; r < Board.Rows; r++)
        {
            for (int c = 0; c < Board.Columns; c++)
            {
                if (board[r, c] == player)
                {
                    // Billy likes the middle!
                    if (c == centerCol) score += 5;
                    
                    // Billy likes building tall towers! (High rows have lower index)
                    score += (Board.Rows - r);
                }
            }
        }

        // Billy is a bit unpredictable
        score += _random.Next(0, 10);
        return score;
    }
}

/// <summary>
/// A 10-year-old understands the basic rules and is starting to look for patterns.
/// They know that getting 3-in-a-row is good and they try to block you if they see you getting 3.
/// </summary>
public class TenYearOld : IHeuristic
{
    public string Name => "Ten-Year-Old (Sarah)";

    public double Evaluate(Player[,] board, Player player)
    {
        double score = 0;
        Player opponent = player == Player.Red ? Player.Yellow : Player.Red;

        // Sarah looks for 2 and 3 in a row
        score += CountLines(board, player, 3) * 50;
        score += CountLines(board, player, 2) * 10;

        // Sarah is smart enough to try and block the opponent's 3-in-a-row
        score -= CountLines(board, opponent, 3) * 40;

        return score;
    }

    private int CountLines(Player[,] board, Player p, int length)
    {
        int count = 0;
        // Horizontal
        for (int r = 0; r < Board.Rows; r++)
            for (int c = 0; c <= Board.Columns - 4; c++)
                if (CheckWindow(board, r, c, 0, 1, p, length)) count++;

        // Vertical
        for (int r = 0; r <= Board.Rows - 4; r++)
            for (int c = 0; c < Board.Columns; c++)
                if (CheckWindow(board, r, c, 1, 0, p, length)) count++;

        // Diagonal
        for (int r = 0; r <= Board.Rows - 4; r++)
        {
            for (int c = 0; c <= Board.Columns - 4; c++)
            {
                if (CheckWindow(board, r, c, 1, 1, p, length)) count++;
                if (CheckWindow(board, r + 3, c, -1, 1, p, length)) count++;
            }
        }
        return count;
    }

    private bool CheckWindow(Player[,] board, int r, int c, int dr, int dc, Player p, int length)
    {
        int match = 0;
        int empty = 0;
        for (int i = 0; i < 4; i++)
        {
            Player cell = board[r + i * dr, c + i * dc];
            if (cell == p) match++;
            else if (cell == Player.None) empty++;
        }
        return match == length && (match + empty == 4);
    }
}

/// <summary>
/// A 14-year-old is competitive and understands the "traps" of Connect 4.
/// They prioritize the center, control the board, and look for "double threats" (two ways to win).
/// </summary>
public class FourteenYearOld : IHeuristic
{
    public string Name => "Fourteen-Year-Old (Alex)";

    public double Evaluate(Player[,] board, Player player)
    {
        double score = 0;
        Player opponent = player == Player.Red ? Player.Yellow : Player.Red;

        // Alex knows the center is the most powerful column
        int centerCol = Board.Columns / 2;
        for (int r = 0; r < Board.Rows; r++)
        {
            if (board[r, centerCol] == player) score += 6;
            else if (board[r, centerCol] == opponent) score -= 6;
        }

        // Alex looks for threats
        score += EvaluateThreats(board, player, opponent);

        return score;
    }

    private double EvaluateThreats(Player[,] board, Player p, Player o)
    {
        double score = 0;
        // Simplified threat evaluation: 
        // 4 in a row: Win
        // 3 in a row with open ends: Very high
        // 2 in a row: Moderate
        
        // This heuristic leverages the same "window" logic but with more aggressive weights
        // similar to Diana Diagonal but tuned for aggressive play.
        
        for (int r = 0; r < Board.Rows; r++)
        {
            for (int c = 0; c < Board.Columns; c++)
            {
                // Check all 4 directions from this cell
                score += GetDirectionalScore(board, r, c, 0, 1, p, o); // H
                score += GetDirectionalScore(board, r, c, 1, 0, p, o); // V
                score += GetDirectionalScore(board, r, c, 1, 1, p, o); // D1
                score += GetDirectionalScore(board, r, c, 1, -1, p, o); // D2
            }
        }
        return score;
    }

    private double GetDirectionalScore(Player[,] board, int r, int c, int dr, int dc, Player p, Player o)
    {
        if (r + 3 * dr < 0 || r + 3 * dr >= Board.Rows || c + 3 * dc < 0 || c + 3 * dc >= Board.Columns)
            return 0;

        int pCount = 0;
        int oCount = 0;

        for (int i = 0; i < 4; i++)
        {
            Player cell = board[r + i * dr, c + i * dc];
            if (cell == p) pCount++;
            else if (cell == o) oCount++;
        }

        if (oCount == 0)
        {
            if (pCount == 4) return 10000;
            if (pCount == 3) return 100;
            if (pCount == 2) return 10;
        }
        else if (pCount == 0)
        {
            if (oCount == 3) return -200; // Alex is very afraid of opponent's 3-in-a-row
            if (oCount == 2) return -20;
        }

        return 0;
    }
}
