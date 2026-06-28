namespace GameHeuristic.Core.Submissions.Y2026;

/// <summary>
/// Team Ilias George
/// </summary>
public class Connect4_2 : IHeuristic
{
    public string Name => "georges 🐷‼️ supa piggy";
    public double Evaluate(Player[,] board, Player player)
    {
        double score = 0;
        for (int r = 0; r < Board.Rows; r++)
        {
            for (int c = 0; c <= Board.Columns - 4; c++)
            {
                score += EvaluateWindow(
                    board[r, c], board[r, c + 1], board[r, c + 2], board[r, c + 3], 
                    player, board);
            }
        }
        for (int c = 0; c < Board.Columns; c++)
        {
            for (int r = 0; r <= Board.Rows - 4; r++)
            {
                score += EvaluateWindow(
                    board[r, c], board[r + 1, c], board[r + 2, c], board[r + 3, c], 
                    player, board);
            }
        }
        for (int r = 0; r <= Board.Rows - 4; r++)
        {
            for (int c = 0; c <= Board.Columns - 4; c++)
            {
                score += EvaluateWindow(
                    board[r, c], board[r + 1, c + 1], board[r + 2, c + 2], board[r + 3, c + 3], 
                    player, board);
            }
        }
        for (int r = 3; r < Board.Rows; r++)
        {
            for (int c = 0; c <= Board.Columns - 4; c++)
            {
                score += EvaluateWindow(
                    board[r, c], board[r - 1, c + 1], board[r - 2, c + 2], board[r - 3, c + 3], 
                    player, board);
            }
        }
        
        Player opponent =
            player == Player.Red
                ? Player.Yellow
                : Player.Red;

        // better high depth results, h2h but weaker leaderboard/tournament. not worth imo
        // becomes an absolute H2H (6 depth and up) tank, but reduces lower depth gains
        // to be most consistent, i simply had to choose to not gaf about single matches
        // focusing on leaderboard and knockout instead. its not perfect which  i hate
        
        int centre = Board.Columns / 2;

        for (int r = 0; r < Board.Rows; r++)
        {
            if (board[r, centre] == player) score += 100;
            else score -= 9999;
        }
        
        return score;
    }
    private double EvaluateWindow(Player p1, Player p2, Player p3, Player p4, Player player, Player[,] board)
    {
        double score = 0;
        
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

        score = -opponentCount + playerCount;
        
        if (playerCount == 3 && emptyCount == 1) score += 1000;
        if (opponentCount == 3 && emptyCount == 1) score -= 12000;
        
        // tournament winner, reduces leaderboard score (NOT WORTH IMO)
        // AFTER 2 hours i can confirm THIS IS NOT WORTH ANYMORE!!
        // if (opponentCount == 0 && playerCount == 2) score += 1000;
        int[] columnWeights =
        {
            1, 4, 8, 16, 8, 4, 1
        };
        
        Player opponent =
            player == Player.Red
                ? Player.Yellow
                : Player.Red;

        for (int r = 0; r < Board.Rows; r++)
        {
            for (int c = 0; c < Board.Columns; c++)
            {
                if (board[r,c] == player)
                    score += columnWeights[c];

                if (board[r,c] == opponent)
                    score -= columnWeights[c];
            }
        }
        
        return score;

        // heres how to crash the program!!
        // return (opponentCount / playerCount) * -1;
    }
}
