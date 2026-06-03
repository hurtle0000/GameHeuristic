using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameHeuristic.Core;
using Microsoft.VisualBasic;

namespace GameHeuristic.Core.Submissions.Baselines;

// A couple of example player heuristics for reference


// Roger Random evaluates every possible move as a different random number
public class Stuartivator : IHeuristic
{
    public string Name { get; set; } = "Stuart Forsyth v1";
    private Random _random = new Random();

    public double Evaluate(Player[,] board, Player player)
    {
        if (player==Player.Red)
        {
            player = Player.Yellow;
        } else
        {
            player = Player.Red;
        }

        StringBuilder b = new StringBuilder();

        for (int col=0; col < Board.Columns; col++)
        {
            StringBuilder r = new StringBuilder();
            for (int row = 0; row < Board.Rows; row++)
            {
                Player p = board[row, col];
                r.Append(ConvertPlayerToString(p));
            }
            b.Append(r.ToString());
            b.Append(" ");
        }

        for (int row = 0; row < Board.Rows; row++) 
        {
            StringBuilder c = new StringBuilder();
            for (int col = 0; col < Board.Columns; col++)
            {
                Player p = board[row, col];
                c.Append(ConvertPlayerToString(p));
            }
            b.Append(c.ToString());
            b.Append(" ");
        }

        b.Append(ConvertPlayerToString(board[2,0])+ConvertPlayerToString(board[3,1])+ConvertPlayerToString(board[4,2])+ConvertPlayerToString(board[5,3]));
        // Debug.WriteLine(b.ToString())
        return _random.NextDouble();
    }

    private string ConvertPlayerToString(Player player)
    {
        if (player == Player.Red)
        {
            return "R";
        }
        else if (player == Player.Yellow)
        {
            return "Y";
        }
        else
        {
            return " ";
        }
    }
}
