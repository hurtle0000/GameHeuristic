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
                score += 5;

        //Rows: Right Wins
        for (int r = 0; r < 3; r++)
        {
            for (int c = 0; c <= Board.Columns - 4; c++)
            {
                if (board[r, c + 1] == enemyPlayer &&
                    board[r, c + 2] == enemyPlayer &&
                    board[r, c + 3] == enemyPlayer)
                    score += 50;

                if (board[r, c + 1] == player &&
                    board[r, c + 2] == player &&
                    board[r, c + 3] == player)
                    score += 500;
            }
        }

        //Rows: Left Wins
        for (int r = 0; r < Board.Rows; r++)
        {
            for (int c = 3; c < Board.Columns; c++)
            {
                if (board[r, c] == enemyPlayer &&
                    board[r, c - 1] == enemyPlayer &&
                    board[r, c - 2] == enemyPlayer &&
                    board[r, c - 3] == enemyPlayer)
                    score += 50;

                if (board[r, c] == player &&
                    board[r, c - 1] == player &&
                    board[r, c - 2] == player &&
                    board[r, c - 3] == player)
                    score += 500;
            }
        }

        //Columns
        for (int c = 0; c < Board.Columns; c++)
        {
            for (int r = 0; r <= Board.Rows - 4; r++)
            {
                if (board[r + 1, c] == enemyPlayer &&
                    board[r + 2, c] == enemyPlayer &&
                    board[r + 3, c] == enemyPlayer)
                    score += 50;

                if (board[r + 1, c] == player &&
                    board[r + 2, c] == player &&
                    board[r + 3, c] == player)
                    score += 500;
            }
        }
        return score;
    }
}
