using GameHeuristic.Core;
using System;
using static System.Formats.Asn1.AsnWriter;

namespace GameHeuristic.Core.Submissions.Baselines;

// A couple of example player heuristics for reference


// GCE test
public class CharlesCode : IHeuristic
{
    public string Name { get; set; } = "Daniel / Charles submission";
    private Random _random = new Random();

    public double Evaluate(Player[,] board, Player player)
    {
        // test test test test


        // Adding the system where it checks everything on the left

        for (int r = 0; r < Board.Rows; r++)
        {
            for (int c = 0; c <= Board.Columns - 4; c++)
            {
                
            }
        }


        return 10.0d;
    }
}