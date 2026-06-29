using GameHeuristic.Core;
 using System;
using static System.Formats.Asn1.AsnWriter;

namespace GameHeuristic.Core.Submissions.Y2026;

public class NicheAlgorithm : IHeuristic
{
    public string Name => "The Nichest Algorithm of all Time";

    public double Evaluate(Player[,] board, Player player)
    {
        Player enemyPlayer = (player == Player.Red) ? Player.Yellow : Player.Red;
        int score = 0;

        for (int r = 0; r < Board.Rows; r++)
            if (board[r, Board.Columns / 2] == player)
                score += 3;

        for (int r = 0; r < 3; r++)
        {
            for (int c = 0; c <= Board.Columns - 4; c++)
            {
                if (board[r,c + 1] == enemyPlayer &&
                    board[r,c + 2] == enemyPlayer &&
                    board[r, c + 3] == enemyPlayer)
                score += 10;
            }
        }

        for (int c = 0; c < Board.Columns; c++)
        {
            for (int r = 0; r <= Board.Rows - 4; r++)
            {
                if (board[r,c + 1] == enemyPlayer &&
                    board[r,c + 2] == enemyPlayer &&
                    board[r, c + 3] == enemyPlayer)
                score += 10;
            }
        }

        return score;
    }
}
