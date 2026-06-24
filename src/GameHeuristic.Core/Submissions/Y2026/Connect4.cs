namespace GameHeuristic.Core.Submissions.Experiment;

/// <summary>
/// Team Ilias
/// Team George
/// </summary>
public class IliasGeorge : IHeuristic
{
    public string Name => "IliasGeorge";
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
        
        if (playerCount == 3 && emptyCount == 1) score += 90;
        if (opponentCount == 3 && emptyCount == 1) score -= 120;
        
        return score;

        // heres how to crash the program!!
        // return (opponentCount / playerCount) * -1;
    }
}
