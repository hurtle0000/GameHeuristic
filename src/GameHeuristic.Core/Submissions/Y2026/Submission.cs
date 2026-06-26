using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameHeuristic.Core.Submissions.Y2026
{
    public class ThomasH : IHeuristic
    {
        public string Name { get; set; } = "Thomas";
        private Random _random = new Random();

        public double Evaluate(Player[,] board, Player player)
        {
            double score = 0;

            for(int i = 0; i < Board.Rows -1; i++)
            {
                for (int j = 0; j < Board.Columns -1; j++)
                {
                    if (board[i,j] == player)
                    {
                       score += SearchAround(i, j, board);
                    }
                    else if (board[i,j] == Player.None)
                    {
                        
                    }
                    else
                    {
                       score -= SearchAround(i, j, board);
                    }

                }
                
            }

            return score;
        }
        public double SearchAround(int starti, int startj, Player[,] board)
        {
            double score = 0;
            double LineScore = 0;
            Player currentplayer = board[starti, startj];
            Player pos = currentplayer;
            for(int i = -1; i < 1; i++)
            {
                for(int j = -1; j < 1; j++)
                {
                    while (pos != currentplayer)
                    {
                        pos = board[starti +i, j + startj];;
                        if (pos == currentplayer)
                        {
                            LineScore++;
                        }
                        else if (pos == Player.None)
                        {
                            LineScore *= 2;
                        }
                        else
                        {
                            LineScore *= 0.5;
                        }
                    }
                    score += LineScore;
                    LineScore = 0;
                    pos = currentplayer;
                }
            }
            return score;
           
        }

    }
}
