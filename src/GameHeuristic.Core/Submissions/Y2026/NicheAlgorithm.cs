using GameHeuristic.Core;
 using System;
using static System.Formats.Asn1.AsnWriter;

namespace GameHeuristic.Core.Submissions.Y2026;

public class NicheAlgorithm : IHeuristic
{
    public string Name => "The Nichest Algorithm of all Time";

    public double Evaluate(Player[,] board, Player player)
    {
        if (board[5,3] == player)
            return 5;
        else if (board[5, 3] == player || board[5, 4] == player)
            return 4;

        
        return 0.0;
    }
}
