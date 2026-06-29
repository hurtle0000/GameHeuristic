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

            for(int i = 1; i < Board.Rows -2; i++)
            {
                for (int j = 1; j < Board.Columns -2; j++)
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

        public double SearchAround(int row, int col, Player[,] board)
        {
            double rowscore = 0;
            int score = 0;
            Player p = board[row,col];
            for(int i = -1;i != 1; i++)
            {
                for (int j = -1; j != 1; j++)
                {
                    if(board[row + i, col + j] == p)
                    {
                        rowscore += 1;
                        rowscore *= 2;
                    }
                    else if(board[row + i, col + j]== Player.None)
                    {
                        rowscore+=1;
                    }
                    else
                    {
                        rowscore--;
                    }
                }
                score += (int)rowscore;
                rowscore = 0;
            }
            return score;
        }

        //public double SearchAround(int i, int j, Player[,] board)
        //{
        //    double score = 0;
        //    double LineScore = 0;
        //    Player currentplayer = board[i, j];
        //    Player pos = currentplayer;
        //    int increment = 0;
        //    //for(int up = -1; up < 1; up++)
        //    //{
        //    //    for(int accr = -1; accr < 1; accr++)
        //    //    {
        //    //        while (pos != currentplayer)
        //    //        {
        //    //            pos = board[i, j - increment];
        //    //            increment++;
        //    //            if (pos == currentplayer)
        //    //            {
        //    //                LineScore++;
        //    //            }
        //    //            else if (pos == Player.None)
        //    //            {
        //    //                LineScore *= 2;
        //    //            }
        //    //            else
        //    //            {
        //    //                LineScore *= 0.5;
        //    //            }
        //    //        }
        //    //        score += LineScore;
        //    //        LineScore = 0;
        //    //    }
        //    //}
        //    while (pos != currentplayer)
        //    {
        //        pos = board[i +increment, j + increment];
        //        increment++;
        //        if (pos == currentplayer)
        //        {
        //            LineScore++;
        //        }
        //        else if (pos == Player.None)
        //        {
        //            LineScore *= 2;
        //        }
        //        else
        //        {
        //            LineScore *= 0.5;
        //        }
        //    }
        //    score += LineScore;
        //    LineScore = 0;
        //    pos = currentplayer;
        //    while (pos != currentplayer)
        //    {
        //        pos = board[i, j + increment];
        //        increment++;
        //        if (pos == currentplayer)
        //        {
        //            LineScore++;
        //        }
        //        else if (pos == Player.None)
        //        {
        //            LineScore *= 2;
        //        }
        //        else
        //        {
        //            LineScore *= 0.5;
        //        }
        //    }
        //    score += LineScore;
        //    LineScore = 0;
        //    pos = currentplayer;
        //    while (pos != currentplayer)
        //    {
        //        pos = board[i - increment, j + increment];
        //        increment++;
        //        if (pos == currentplayer)
        //        {
        //            LineScore++;
        //        }
        //        else if (pos == Player.None)
        //        {
        //            LineScore *= 2;
        //        }
        //        else
        //        {
        //            LineScore *= 0.5;
        //        }
        //    }
        //    score += LineScore;
        //    LineScore = 0;
        //    pos = currentplayer;
        //    while (pos != currentplayer)
        //    {
        //        pos = board[i + increment, j];
        //        increment++;
        //        if (pos == currentplayer)
        //        {
        //            LineScore++;
        //        }
        //        else if (pos == Player.None)
        //        {
        //            LineScore *= 2;
        //        }
        //        else
        //        {
        //            LineScore *= 0.5;
        //        }
        //    }
        //    score += LineScore;
        //    LineScore = 0;
        //    pos = currentplayer;
        //    while (pos != currentplayer)
        //    {
        //        pos = board[i, j];
        //        increment++;
        //        if (pos == currentplayer)
        //        {
        //            LineScore++;
        //        }
        //        else if (pos == Player.None)
        //        {
        //            LineScore *= 2;
        //        }
        //        else
        //        {
        //            LineScore *= 0.5;
        //        }
        //    }
        //    score += LineScore;
        //    LineScore = 0;
        //    pos = currentplayer;
        //    while (pos != currentplayer)
        //    {
        //        pos = board[i - increment, j];
        //        increment++;
        //        if (pos == currentplayer)
        //        {
        //            LineScore++;
        //        }
        //        else if (pos == Player.None)
        //        {
        //            LineScore *= 2;
        //        }
        //        else
        //        {
        //            LineScore *= 0.5;
        //        }
        //    }
        //    score += LineScore;
        //    LineScore = 0;
        //    pos = currentplayer;
        //    while (pos != currentplayer)
        //    {
        //        pos = board[i + increment, j - increment];
        //        increment++;
        //        if (pos == currentplayer)
        //        {
        //            LineScore++;
        //        }
        //        else if (pos == Player.None)
        //        {
        //            LineScore *= 2;
        //        }
        //        else
        //        {
        //            LineScore *= 0.5;
        //        }
        //    }
        //    score += LineScore;
        //    LineScore = 0;
        //    pos = currentplayer;
        //    while (pos != currentplayer)
        //    {
        //        pos = board[i, j - increment];
        //        increment++;
        //        if (pos == currentplayer)
        //        {
        //            LineScore++;
        //        }
        //        else if (pos == Player.None)
        //        {
        //            LineScore *= 2;
        //        }
        //        else
        //        {
        //            LineScore *= 0.5;
        //        }
        //    }
        //    score += LineScore;
        //    LineScore = 0;
        //    pos = currentplayer;
        //    while (pos != currentplayer)
        //    {
        //        pos = board[i - increment, j - increment];
        //        increment++;
        //        if (pos == currentplayer)
        //        {
        //            LineScore++;
        //        }
        //        else if (pos == Player.None)
        //        {
        //            LineScore *= 2;
        //        }
        //        else
        //        {
        //            LineScore *= 0.5;
        //        }
        //    }
        //    score += LineScore;
        //    LineScore = 0;
        //    pos = currentplayer;

        //    return score;
        //}

    }
}
