using System;
using GameHeuristic.Core;

namespace GameHeuristic.Core.Submissions.Baselines;

// A couple of example player heuristics for reference


// GCE test
public class JoshuaKarlBot : IHeuristic
{
    public string Name { get; set; } = "JoshuaKarlBot";
    private Random _random = new Random();

    public double Evaluate(Player[,] board, Player player)
    {
        double score = 0;
        Player enemy = Player.None;
        if (player == Player.Red)
        {
            enemy = Player.Yellow;
        }
        else
        {
            enemy = Player.Red;
        }
        int[,] bonus = new int[board.GetLength(0), board.GetLength(1)];
        for (int a = 0; a < board.GetLength(0); a++)
        {
            for (int b = 0; b < board.GetLength(1); b++ )
            {
                if (board[a,b] == player)
                {
                    for (int c = -1; c <= 1; c++)
                    {
                        for (int d = -1; d <= 1; d++)
                        {
                            if ((a + c > 0 && a + c < board.GetLength(0)) && (b + d > 0 && b + d < board.GetLength(1)))
                            {
                                if (board[a + c, b + d] == player && ((c == 0) && (d == 0)))
                                {
                                    score += 1;
                                    bonus[a,b] += 1;
                                    if ((a + (2 * c) > 0 && a + (3 * c) < board.GetLength(0)) && (b + (2 * d) > 0 && b + (3 * d) < board.GetLength(1)))
                                    {
                                        if (board[a + c + c, b + d + d] != enemy && (board[a + c + c + c, b + d + d + d] != enemy))
                                        {
                                            score += 2;
                                        }
                                    }
                                }
                                else if (board[a + c, b + d] == Player.None)
                                {
                                    score += 0.1;
                                }
                            }
                        }
                    }
                }
            }
        }
        for (int a = 0; a < board.GetLength(0); a++)
        {
            for (int b = 0; b < board.GetLength(1); b++)
            {
                if (board[a, b] == enemy)
                {
                    for (int c = -1; c <= 1; c++)
                    {
                        for (int d = -1; d <= 1; d++)
                        {
                            if ((a + c > 0 && a + c < board.GetLength(0)) && (b + d > 0 && b + d < board.GetLength(1)))
                            {
                                if (board[a + c, b + d] == enemy && ((c == 0) && (d == 0)))
                                {
                                    score -= 0.9;
                                    bonus[a, b] -= 1;
                                    if ((a + (3 * c) > 0 && a + (3 * c) < board.GetLength(0)) && (b + (3 * d) > 0 && b + (3 * d) < board.GetLength(1)))
                                    {
                                        if ((board[a + c + c, b + d + d] == enemy) && (board[a + c + c + c, b + d + d + d] != player)) 
                                        {
                                            score -= 7;
                                        }
                                        else if ((board[a + c + c, b + d + d] != player) && (board[a + c + c + c, b + d + d + d] == enemy))
                                        {
                                            score -= 6;
                                        }
                                        else if ((board[a + c + c, b + d + d] != player) && (board[a + c + c + c, b + d + d + d] != player))
                                        {
                                            score -= 5;
                                        }
                                    }
                                }
                                else if (board[a + c, b + d] == Player.None)
                                {
                                    score -= 0.11;
                                }
                            }
                        }
                    }
                }
            }
        }
        for (int a = 0; a < board.GetLength(0); a++)
        {
            for (int b = 0; b < board.GetLength(1); b++)
            {
                switch(bonus[a,b])
                {
                    case 2:
                        score += 1;
                        break;
                    case >2:
                        score += 2;
                        break;
                    case -2:
                        score -= 2;
                        break;
                    case < -2:
                        score -= 5;
                        break;
                    case -1:
                        score -= 0.05;
                        break;
                    default:
                        break;
                }
            }
        }



        return score;
    }
}