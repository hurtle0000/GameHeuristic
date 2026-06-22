using GameHeuristic.Core;
 using System;
using static System.Formats.Asn1.AsnWriter;

namespace GameHeuristic.Core.Submissions.Y2026;

public class NicheAlgorithm : IHeuristic
{
    public string Name => "The Nichest Algorithm of all Time";

    public double Evaluate(Player[,] board, Player player)
    {
        for (int r = 0; r < Board.Rows; r++)
        {
            for (int c = 0; c <= Board.Columns; c++)
            {
                if (board[r, c] != player && board[r, c] != Player.None)
                    return 10.0;
            }
        }
        return 0.0;
    }

}
