using System;
using GameHeuristic.Core;

namespace GameHeuristic.Core.Submissions.Y2026;

/// <summary>
/// A sample student submission for the class of 2026.
/// This bot prioritizes the middle columns where most Connect 4 winning lines intersect.
/// </summary>
public class StudentHeuristic2026 : IHeuristic
{
    public string Name => "Student 2026 - Center Heavy";

    public double Evaluate(Player[,] board, Player player)
    {
        double score = 0;
        Player opponent = player == Player.Red ? Player.Yellow : Player.Red;

        // Columns weighted by their distance to the center column
        // Col 3 (Center): 4 points
        // Cols 2, 4:      3 points
        // Cols 1, 5:      2 points
        // Cols 0, 6:      1 point
        int[] columnWeights = { 1, 2, 3, 4, 3, 2, 1 };

        for (int r = 0; r < Board.Rows; r++)
        {
            for (int c = 0; c < Board.Columns; c++)
            {
                if (board[r, c] == player)
                {
                    score += columnWeights[c];
                }
                else if (board[r, c] == opponent)
                {
                    score -= columnWeights[c];
                }
            }
        }

        return score;
    }
}
