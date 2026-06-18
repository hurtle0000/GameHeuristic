 using System;
using GameHeuristic.Core;

namespace GameHeuristic.Core.Submissions.Y2026;

/// <summary>
/// A sample student submission for the class of 2026.
/// This bot greedily tries to build its own lines of 2 completely ignoring the opponent's moves.
/// </summary>
public class CadanAndEli : IHeuristic
{
    public string Name => "CadanAndEli";

    /// <summary>
    /// 
    /// </summary>
    /// <param name="board"></param>
    /// <param name="player"></param>
    /// <returns></returns>
    public double Evaluate(Player[,] board, Player player)
    {
        // Setting up useful variables such as score and enemy player
        Player enemy = Player.None;
        double score = 0;
        if (player == Player.Red)
            enemy = Player.Yellow;
        else
            enemy = Player.Red;
        Player empty = Player.None;

        // This is the board we play on.
        //   0 1 2 3 4 5 6
        //  +-------------
        // 0|X X X X - - -  
        // 1|X X X X - - - 
        // 2|X X X X - - -
        // 3|X X X X - - -
        // 4|X X X X - - -
        // 5|X X X X - - -

        // Heavily insentives AI to always pick [5,3] to start with.
        if (board[5, 3] == player)
        {
            score += 1000;
            //tries to get AI to place in tower on centre square.
            for (int r = Board.Rows - 1; r <= 0; r--)
                if (board[r, 3] == player)
                {
                    score += 2;
                }

        }

        //For when enemy chooses best move to start.
        else  if (board[5,3] == enemy)
        {
            //These two moves are the best second moves when enemy player center.
            if (board[5,2] == player)
            {
                score += 10;
            }
            if (board[5,4] == player)
            {
                score += 10;
            }
        }


        #region Glenn Code
        // Evaluate all possible 4-slot windows (horizontal, vertical, diagonal)

        // Horizontal - for each row on the board, look at the lines of 4 to the right and calculate a score
        // 
        // Only the windows marked X will be included in the loop
        //
        //   0 1 2 3 4 5 6
        //  +-------------
        // 0|X X X X - - -  
        // 1|X X X X - - - 
        // 2|X X X X - - -
        // 3|X X X X - - -
        // 4|X X X X - - -
        // 5|X X X X - - -
        //
        for (int r = 0; r < Board.Rows; r++)
            {
                for (int c = 0; c <= Board.Columns - 4; c++)
                {
                    score += EvaluateWindow(
                        board[r, c], board[r, c + 1], board[r, c + 2], board[r, c + 3],
                        player);
                }
            }

            // Vertical - for each column on the board, look at the lines of 4 below and calculate a score
            // 
            // Only the windows marked X will be included in the loop
            //
            //   0 1 2 3 4 5 6
            //  +-------------
            // 0|X X X X X X X  
            // 1|X X X X X X X 
            // 2|X X X X X X X
            // 3|- - - - - - -
            // 4|- - - - - - -
            // 5|- - - - - - -
            //
            for (int c = 0; c < Board.Columns; c++)
            {
                for (int r = 0; r <= Board.Rows - 4; r++)
                {
                    score += EvaluateWindow(
                        board[r, c], board[r + 1, c], board[r + 2, c], board[r + 3, c],
                        player);
                }
            }

            // Diagonal (Down-Right)- for each column on the board, look at the lines of 4 down-right and calculate a score
            // 
            // Only the windows marked X will be included in the loop
            //
            //   0 1 2 3 4 5 6
            //  +-------------
            // 0|X X X X - - -  
            // 1|X X X X - - - 
            // 2|X X X X - - -
            // 3|- - - - - - -
            // 4|- - - - - - -
            // 5|- - - - - - -
            //
            for (int r = 0; r <= Board.Rows - 4; r++)
            {
                for (int c = 0; c <= Board.Columns - 4; c++)
                {
                    score += EvaluateWindow(
                        board[r, c], board[r + 1, c + 1], board[r + 2, c + 2], board[r + 3, c + 3],
                        player);
                }
            }

            // Diagonal (Up-Right)- for each column on the board, look at the lines of 4 up-right and calculate a score
            // 
            // Only the windows marked X will be included in the loop
            //
            //   0 1 2 3 4 5 6
            //  +-------------
            // 0|- - - - - - -  
            // 1|- - - - - - - 
            // 2|- - - - - - -
            // 3|X X X X - - -
            // 4|X X X X - - -
            // 5|X X X X - - -
            //
            for (int r = 3; r < Board.Rows; r++)
            {
                for (int c = 0; c <= Board.Columns - 4; c++)
                {
                    score += EvaluateWindow(
                        board[r, c], board[r - 1, c + 1], board[r - 2, c + 2], board[r - 3, c + 3],
                        player);
                }
            }

            return score;
        }
    
    /// <summary>
    /// This is the calculation, which relies on having a set of 4 co-ordinates for a possible winning
    /// line sent as parameters.
    ///
    /// Lines with two tokens score 10

    ///
    /// It doesn't matter where the spaces are in the lines of two
    ///
    /// If there is an opponent token anywhere in the line, it's not a possible winner so is set to 0
    /// regardless of the number of player tokens
    /// </summary>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <param name="p3"></param>
    /// <param name="p4"></param>
    /// <param name="player"></param>
    /// <returns></returns>
    private double EvaluateWindow(Player p1, Player p2, Player p3, Player p4, Player player)
    {
        int playerCount = 0;
        int opponentCount = 0;
        int emptyCount = 0;

        Player[] window = { p1, p2, p3, p4 };
        foreach (Player p in window)
        {
            if (p == player) playerCount++;
            else if (p == Player.None) emptyCount++;
            else opponentCount++;
        }

        if (playerCount == 2 && emptyCount == 2)
        {
            
            if (playerCount ==3 && emptyCount ==2)
            {
                return 4.0;
            }
            
            
            return 2.0;
        
        
        }  // Prioritize lines of 2
       

            return 0;
        #endregion
    }
}
