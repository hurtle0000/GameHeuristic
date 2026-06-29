using GameHeuristic.Core;
 using System;
using static System.Formats.Asn1.AsnWriter;

namespace GameHeuristic.Core.Submissions.Y2026;

public class NicheAlgorithm : IHeuristic
{
    public string Name => "The Nichest Algorithm of all Time";

    public double Evaluate(Player[,] board, Player player)
    {
        Player enemyPlayer;
        if (player == Player.Red)
            enemyPlayer = Player.Yellow;
        else
            enemyPlayer = Player.Red;

        if (board[5, 3] == player)
            return 5;
        else if (board[5, 3] == player || board[5, 4] == player)
            return 4;

        for (int r = 0; r < 3; r++)
        {
            for (int c = 0; c <= Board.Columns - 4; c++)
            {
                if (board[r,c + 1] == enemyPlayer &&
                    board[r,c + 2] == enemyPlayer &&
                    board[r, c + 3] == enemyPlayer)
                return 10;
            }
        }

        return 0.0;
    }
}
