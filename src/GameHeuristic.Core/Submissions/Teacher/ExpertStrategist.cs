using System;
using GameHeuristic.Core;

namespace GameHeuristic.Core.Submissions.Teacher;

public class ExpertStrategist : IHeuristic
{
    public string Name => "Expert Strategist (Teacher)";

    public double Evaluate(Player[,] board, Player player)
    {
        double score = 0;
        Player opponent = player == Player.Red ? Player.Yellow : Player.Red;

        int centerCol = Board.Columns / 2;
        for (int r = 0; r < Board.Rows; r++)
        {
            if (board[r, centerCol] == player) score += 30;
            else if (board[r, centerCol] == opponent) score -= 30;
        }

        score += EvaluateAllWindows(board, player, opponent);

        return score;
    }

    private double EvaluateAllWindows(Player[,] board, Player p, Player o)
    {
        double score = 0;

        for (int r = 0; r < Board.Rows; r++)
            for (int c = 0; c <= Board.Columns - 4; c++)
                score += ScoreWindow(board[r, c], board[r, c + 1], board[r, c + 2], board[r, c + 3], p, o, r);

        for (int c = 0; c < Board.Columns; c++)
            for (int r = 0; r <= Board.Rows - 4; r++)
                score += ScoreWindow(board[r, c], board[r + 1, c], board[r + 2, c], board[r + 3, c], p, o, r);

        for (int r = 0; r <= Board.Rows - 4; r++)
            for (int c = 0; c <= Board.Columns - 4; c++)
                score += ScoreWindow(board[r, c], board[r + 1, c + 1], board[r + 2, c + 2], board[r + 3, c + 3], p, o, r);

        for (int r = 3; r < Board.Rows; r++)
            for (int c = 0; c <= Board.Columns - 4; c++)
                score += ScoreWindow(board[r, c], board[r - 1, c + 1], board[r - 2, c + 2], board[r - 3, c + 3], p, o, r);

        return score;
    }

    private double ScoreWindow(Player p1, Player p2, Player p3, Player p4, Player player, Player opponent, int row)
    {
        int pCount = 0;
        int oCount = 0;
        int empty = 0;

        Player[] window = { p1, p2, p3, p4 };
        foreach (Player p in window)
        {
            if (p == player) pCount++;
            else if (p == opponent) oCount++;
            else empty++;
        }

        double rowBonus = (row % 2 == 0) ? 1.2 : 1.0;

        if (pCount == 4) return 100000;
        if (pCount == 3 && empty == 1) return 500 * rowBonus;
        if (pCount == 2 && empty == 2) return 50;

        if (oCount == 3 && empty == 1) return -800;
        if (oCount == 2 && empty == 2) return -40;

        return 0;
    }
}
