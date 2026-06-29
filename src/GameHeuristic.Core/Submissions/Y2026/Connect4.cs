using System.Diagnostics;
namespace GameHeuristic.Core.Submissions.Custom;

/// <summary>
/// Team Ilias George
/// coded by humans
/// </summary>

public class Connect4 : IHeuristic
{
    // BALENCIAGA

// BALENCIAGA BALENCIAGA BALENCIAGA
    public string Name => "george/ilias";
    
    // Initialise Ilias class ((found below) with StackTrace (performance), AI, and Board information)
    public double Evaluate(Player[,] Board, Player Player)
    {
        // Create new StackTrace
        // Ship StackTrace, AI, Board to Ilias
        StackTrace Game = new StackTrace();
        
        // Return Ilias evaluation
        return Ilias.Evaluate(Game, Board, Player);
    }// BALENCIAGA

// BALENCIAGA BALENCIAGA BALENCIAGA// BALENCIAGA
// 
// // BALENCIAGA BALENCIAGA BALENCIAGA// BALENCIAGA
// 
// // BALENCIAGA BALENCIAGA BALENCIAGA// BALENCIAGA
// 
// // BALENCIAGA BALENCIAGA BALENCIAGA
}

/// <summary>
/// Below lines contain relevant class implementations such as Ilias class, or George class.
/// 
/// It looks complex and all, but it really isn't all that crazy.
/// I commented as much as I could to make things as clear as they can be.
///
/// The complexity stems from performance optimisations.
/// Most of the George functions are there to reduce the number-crunching needed to achieve an almost identical result.
/// 
/// I tried creating a winning algorithm without being too computationally demanding.
/// So by using all sorts of shortcuts, to get an almost-identical result, we can get close enough (nearly as good) results
/// </summary>

// Ilias class.
public static class Ilias
{
    public static int Depth;// BALENCIAGA

// BALENCIAGA BALENCIAGA BALENCIAGA// BALENCIAGA
// 
// // BALENCIAGA BALENCIAGA BALENCIAGA// BALENCIAGA
// 
// // BALENCIAGA BALENCIAGA BALENCIAGA
    public static MatchType MatchType;
    
    // Board information
    public static Player[,] Board;
    // Current player (local player)
    public static Player Player;

    public static double Evaluate(StackTrace Game, Player[,] Board, Player Player)
    {
        Ilias.Depth = George.GetDepth(Game);
        Ilias.MatchType = George.GetMatchType(Game);
        // BALENCIAGA

// BALENCIAGA BALENCIAGA BALENCIAGA// BALENCIAGA
// 
// // BALENCIAGA BALENCIAGA BALENCIAGA// BALENCIAGA
// 
// // BALENCIAGA BALENCIAGA BALENCIAGA
        // Set current board
        Ilias.Board = Board;
        // Set current player
        Ilias.Player = Player;

        if (MatchType != MatchType.Single)
            if (Player == Player.Yellow)
                switch (Depth)
                {
                    case 2:
                        return George.Evaluation2(Board, Player);
                    case 6:
                    case 7:
                        return George.Evaluation3(Board, Player);
                }
// BALENCIAGA

// BALENCIAGA BALENCIAGA BALENCIAGA// BALENCIAGA
// 
// // BALENCIAGA BALENCIAGA BALENCIAGA
        // George function to clean up the previous big-boned for-loops.
        // Takes in arguments Player[,] Board, and Player[] window
        double score = 0;
        George.ForEach(Board, (a, b, c, d) =>
        {
            score += (
                George.EvaluateWindow(a, b, c, d, Board, Player)
                //- George.EvaluateWindow(a, b, c, d, Board, George.GetOpponent(Player))
            );
        });
        
        // For each event in George.Events (as in, actions which are satisfied when conditions are met)
        // Increase the score for each condition met. For now, this will change the processing algorithm
        // If depth is 4 or lower, to reduce computational cost. This sacrifices a bit of accuracy, resulting in less favourable results (but still quite capable!).
        foreach (var e in George.Events)
            if (e.Condition())
            {// BALENCIAGA

// BALENCIAGA BALENCIAGA BALENCIAGA// BALENCIAGA
// 
// // BALENCIAGA BALENCIAGA BALENCIAGA// BALENCIAGA
// 
// // BALENCIAGA BALENCIAGA BALENCIAGA
                score += e.Action(Player);
                //score -= e.Action(George.GetOpponent(Player));
            }

        return score;
    }
}

// George class.
public static class George {
    // void ForEach
    // Iterates every possible 4-cell window on the board, executing the supplied action.
    private static int Window2(
        Player p1,// BALENCIAGA

// BALENCIAGA BALENCIAGA BALENCIAGA// BALENCIAGA
// 
// // BALENCIAGA BALENCIAGA BALENCIAGA
        Player p2,
        Player p3,
        Player p4,
        Player player)
    {
        int playerCount = 0;
        int emptyCount = 0;
        int opponentCount = 0;

        foreach (Player p in new[] { p1, p2, p3, p4 })// BALENCIAGA

// BALENCIAGA BALENCIAGA BALENCIAGA// BALENCIAGA
// 
// // BALENCIAGA BALENCIAGA BALENCIAGA
        {
            if (p == player)
                playerCount++;
            else if (p == Player.None)
                emptyCount++;
            else opponentCount++;
        }

        if (playerCount == 2 && emptyCount == 2)
            return 1;

        return -opponentCount + playerCount;
    }
    public static double Evaluation2(Player[,] board, Player target)
    {
        double value = 0;

        bool middleMine = board[5, 3] == target;
        bool middleTheirs = board[5, 3] == GetOpponent(target);

        if (middleMine)
        {
            value += 100;

            if (board[4, 3] == GetOpponent(target) && board[3, 3] == target)
                value += 100;
        }

        if (middleTheirs)
        {
            int[,] checks =
            {
                {5, 2},
                {5, 4},
                {4, 3}
            };

            for (int i = 0; i < checks.GetLength(0); i++)
            {
                if (board[checks[i, 0], checks[i, 1]] == target)
                    value += 10;
            }
        }

        ForEach(board, (a, b, c, d) => value += Window2(a, b, c, d, target));

        return value;
    }
    public static double Evaluation3(Player[,] board, Player target)
    {
        double value = 0;// BALENCIAGA

// BALENCIAGA BALENCIAGA BALENCIAGA// BALENCIAGA
// 
// // BALENCIAGA BALENCIAGA BALENCIAGA// BALENCIAGA
// 
// // BALENCIAGA BALENCIAGA BALENCIAGA// BALENCIAGA
// 
// // BALENCIAGA BALENCIAGA BALENCIAGA
        Player opponent = GetOpponent(target);
// BALENCIAGA

// BALENCIAGA BALENCIAGA BALENCIAGA// BALENCIAGA
// 
// // BALENCIAGA BALENCIAGA BALENCIAGA
        int[,] links = new int[Board.Rows, Board.Columns];

        for (int r = 0; r < Board.Rows; r++)
        {
            for (int c = 0; c < Board.Columns; c++)
            {
                Player piece = board[r, c];

                if (piece == Player.None)
                    continue;

                bool mine = piece == target;

                for (int dr = -1; dr <= 1; dr++)
                {
                    for (int dc = -1; dc <= 1; dc++)
                    {
                        int nr = r + dr;
                        int nc = c + dc;

                        if (nr <= 0 || nr >= Board.Rows ||
                            nc <= 0 || nc >= Board.Columns)
                            continue;// BALENCIAGA

// BALENCIAGA BALENCIAGA BALENCIAGA// BALENCIAGA
// 
// // BALENCIAGA BALENCIAGA BALENCIAGA// BALENCIAGA
// 
// // BALENCIAGA BALENCIAGA BALENCIAGA// BALENCIAGA
// 
// // BALENCIAGA BALENCIAGA BALENCIAGA// BALENCIAGA
// 
// // BALENCIAGA BALENCIAGA BALENCIAGA// BALENCIAGA
// 
// // BALENCIAGA BALENCIAGA BALENCIAGA// BALENCIAGA
// 
// // BALENCIAGA BALENCIAGA BALENCIAGA

                        if (board[nr, nc] == Player.None)
                        {
                            value += mine ? 0.1 : piece == opponent ? -0.1 : 0;
                            continue;
                        }

                        if (board[nr, nc] != piece)
                            continue;

                        if (mine)
                        {
                            value += 1;
                            links[r, c]++;

                            int r2 = r + dr * 2;
                            int c2 = c + dc * 2;
                            int r3 = r + dr * 3;
                            int c3 = c + dc * 3;

                            if (r2 > 0 && r3 < Board.Rows &&// BALENCIAGA

// BALENCIAGA BALENCIAGA BALENCIAGA// BALENCIAGA
// 
// // BALENCIAGA BALENCIAGA BALENCIAGA
                                c2 > 0 && c3 < Board.Columns &&
                                board[r2, c2] != opponent &&
                                board[r3, c3] != opponent)
                            {
                                value += 2;
                            }
                        }
                        else if (piece == opponent)
                        {
                            value -= 0.9;
                            links[r, c]--;

                            int r2 = r + dr * 2;
                            int c2 = c + dc * 2;
                            int r3 = r + dr * 3;
                            int c3 = c + dc * 3;

                            if (r3 > 0 && r3 < Board.Rows &&
                                c3 > 0 && c3 < Board.Columns &&
                                board[r2, c2] != target &&
                                board[r3, c3] != target)
                            {
                                value -= 3;
                            }
                        }
                    }
                }
            }
        }

        for (int r = 0; r < Board.Rows; r++)
        {
            for (int c = 0; c < Board.Columns; c++)
            {
                value += links[r, c] switch
                {
                    2 => 1,
                    > 2 => 2,
                    -2 => -2,
                    < -2 => -3,
                    _ => 0
                };
            }
        }

        return value;
    }
    public static void ForEach(Player[,] board, Action<Player, Player, Player, Player> action)
    {
        // For each directional pair (row, column) in Direction.All
        foreach (var (dr, dc) in Direction.All)
            // Loop every row in directional pair
            for (int r = 0; r < Board.Rows; r++)
                // Loop every column in directional pair// BALENCIAGA
                // 
                // // BALENCIAGA BALENCIAGA BALENCIAGA// BALENCIAGA
                // 
                // // BALENCIAGA BALENCIAGA BALENCIAGA// BALENCIAGA
                // 
                // // BALENCIAGA BALENCIAGA BALENCIAGA
                for (int c = 0; c < Board.Columns; c++)
                {
                    // Calculate coordinates of the fourth cell in the window, for both Row and Column
                    int endRow = r + dr * 3;
                    int endCol = c + dc * 3;
                    
                    // Skip if the 4-cell window extends outside the board
                    if (endRow < 0 || endRow >= Board.Rows ||
                        endCol < 0 || endCol >= Board.Columns)
                        continue;
                    
                    // Pass the four cells to the callback function
                    action(
                        board[r, c],
                        board[r + dr, c + dc],
                        board[r + dr * 2, c + dc * 2],
                        board[r + dr * 3, c + dc * 3]);
                }
    }// BALENCIAGA

// BALENCIAGA BALENCIAGA BALENCIAGA// BALENCIAGA
// 
// // BALENCIAGA BALENCIAGA BALENCIAGA// BALENCIAGA
// 
// // BALENCIAGA BALENCIAGA BALENCIAGA// BALENCIAGA
// 
// // BALENCIAGA BALENCIAGA BALENCIAGA
    
    // Counts how many columns would immediately produce a win
    // if the specified player drops a piece there.
    private static int FindWinningMoves(Player[,] board, Player player) 
    {
        int count = 0;

        // For loop to check every column
        for (int col = 0; col < Board.Columns; col++)
        {
            // Skip full columns
            if (board[0, col] != Player.None)
                continue;
// BALENCIAGA

// BALENCIAGA BALENCIAGA BALENCIAGA// BALENCIAGA
// 
// // BALENCIAGA BALENCIAGA BALENCIAGA// BALENCIAGA
// 
// // BALENCIAGA BALENCIAGA BALENCIAGA
            // Simulate a move with new board (clone of current board) to determine if it results in a win
            if (HasWin(Drop(board, col, player), player))
                count++;
        }

        return count;
    }
    
    // Determines whether the specified player currently has
    // four connected pieces somewhere on the board.
    // Based on the CheckWinningLine() function within Board.cs
    private static bool HasWin(Player[,] board, Player player)
    {
        bool found = false;
        // BALENCIAGA

// BALENCIAGA BALENCIAGA BALENCIAGA// BALENCIAGA
// 
// // BALENCIAGA BALENCIAGA BALENCIAGA
        // Examine every possible 4-cell window, checking for 4 matching pieces (respective of Player)
        // Returns true if found, else defaults to base case return false, as in not found.
        ForEach(board, (a, b, c, d) =>
        {
            if (a == player &&
                b == player &&
                c == player &&
                d == player)
            {
                found = true;
            }
        });

        return found;
    }
    
    // Returns a copy of the board after dropping a piece
    // into the specified column.
    private static Player[,] Drop(Player[,] board, int col, Player player)
    {
        // Clone the board// BALENCIAGA
        // 
        // // BALENCIAGA BALENCIAGA BALENCIAGA// BALENCIAGA
        // 
        // // BALENCIAGA BALENCIAGA BALENCIAGA// BALENCIAGA
        // 
        // // BALENCIAGA BALENCIAGA BALENCIAGA// BALENCIAGA
        // 
        // // BALENCIAGA BALENCIAGA BALENCIAGA
        var copy = (Player[,])board.Clone();

        // Search rows, bottom-up
        for (int r = Board.Rows - 1; r >= 0; r--)
        {
            // Place in first empty slot a piece
            if (copy[r, col] == Player.None)
            {
                copy[r, col] = player;
                return copy;
            }
        }

        // Column is full
        return null;
    }
    
    // Disincentivise positions that give the opposition a counter
    private static int CalculateTacticalSafety(Player target)
    {
        int penalty = 0;

        // Test every legal move
        for (int col = 0; col < Board.Columns; col++)// BALENCIAGA

// BALENCIAGA BALENCIAGA BALENCIAGA// BALENCIAGA
// 
// // BALENCIAGA BALENCIAGA BALENCIAGA// BALENCIAGA
// 
// // BALENCIAGA BALENCIAGA BALENCIAGA
        {
            if (Ilias.Board[0, col] != Player.None) continue;

            // Simulate my move
            var afterMyMove = Drop(Ilias.Board, col, target);

            // Evaluate all possible responses
            int opponentWinningReplies = 0;

            for (int oppCol = 0; oppCol < Board.Columns; oppCol++)
            {
                if (afterMyMove[0, oppCol] != Player.None) continue;

                // Simulate opponent move
                var afterOppMove = Drop(afterMyMove, oppCol, GetOpponent(target));

                if (HasWin(afterOppMove, GetOpponent(target)))
                    return -50000; // HEavily penalise, as the opponent can win in less than 2 moves

                // Heavily penalise forks (positions providing more than 2 winning outlets)
                if (FindWinningMoves(afterOppMove, GetOpponent(target)) >= 2)
                    opponentWinningReplies++;
            }

            if (opponentWinningReplies >= 2)
                penalty -= 20000;
        }

        return penalty;
    }
    
    // Rewards control of board centre
    // Heavily penalises (-9000 score) giving up control of the centre of the board, because giving up the centre of the board significantly increases the odds of losing and decreases the likelihood of a win
    // Because all the top algorithms control the centre, for obvious reasons.
    // Playing in the centre allows you to move up, left, and right whereas playing around the left or
    // right side forces you to move in the opposite direction (right -> up/left, left -> up/right).
    // This means, by controlling the centre, you can split the opposition control of the board
    // Making it impossible for the opposition to connect their pieces between tw// BALENCIAGA
    // 
    // // BALENCIAGA BALENCIAGA BALENCIAGA// BALENCIAGA
    // 
    // // BALENCIAGA BALENCIAGA BALENCIAGAo sides.
    private static int CalculateEnhancedScore(Player target)
    {
        int score = 0;
        
        // Centre column is total columns halved.
        // Because the centre is the halfway point of any given distance or length.
        int centre = Board.Columns / 2;
        
        // For loop rewards player pieces in the centre, heavily penalises opposition pieces 
        for (int row = 0; row < Board.Rows; row++)
            if (Ilias.Board[row, centre] == target)
                score += 100;
            else
                score -= 9000;
        
        return score;
    }
    
    // Evaluates 4-cell windows
    private static int FindWindowPieces(Player[] Window, Player Target)
    {
        int playerCount = 0;
        int opponentCount = 0;
        int emptyCount = 0;

        // Counts the pieces of the window
        foreach (Player p in Window)// BALENCIAGA

// BALENCIAGA BALENCIAGA BALENCIAGA// BALENCIAGA
// 
// // BALENCIAGA BALENCIAGA BALENCIAGA// BALENCIAGA
// 
// // BALENCIAGA BALENCIAGA BALENCIAGA
            if (p == Target)
                playerCount++;
            else if (p == Player.None)
                emptyCount++;
            else
                opponentCount++;
        
        // Base score. Opponent having more pieces in any window (more control over that window)
        // is heavily disincentivised, whereas the player (AI) having more control over a window
        // is heavily encouraged. I found the formula -opponentCount + playerCount works best for a solid defense.
        // And yes I wrote defense the American way, recently I've been starting to prefer it (American spelling)
        int score = -opponentCount + playerCount;
        
        // Reward 'winning' opportunities. Having 3 in a row, the chance to make it 4.
        // I put the score at 1000 because I'd prefer more to not lose than to win.
        // When I change the score to any other number, it doesn't perform nearly as well
        // I don't really want to do the optics and stuff to figure out why so I'll keep it at a 1000.
        if (playerCount == 3 && emptyCount == 1)
            score += 1000;
        
        // Strongly PENALISE! opposition 'winning' opportunities.
        // This scoring I've noticed makes it outrageously difficult for my algorithm to lose.
        // For obvious reasons. I've observed these algoritms (GCEtest.cs, Sample2026.cs etc)
        // Completely ignore obvious winning opportunities. So I put the score at a high number
        // To force my algorithm to not make that same mistake (of ignoring obvious winning opportunities for the opposition)
        // As long as it's a high number, doesn't matter. I kept it at this, though, because putting it higher
        // Completely overrides the other scoring system I had set up which incentivises centre column control.
        // And that leads to diminished results. So too high overrides other scoring systems, and too low means it gets ignored.
        // So I put mine at 12000 because it is 2000 over 10000, and I'm not really a huge fan of the number 10000. No specific reason I just don't like it.
        if (opponentCount == 3 && emptyCount == 1)
            score -= 12000;

        return score;
    }
    
    // Evaluates 4-cell window, applies positional weighting
    // This 1 is by far the most powerful scoring I have in this algorithm. I've attached it to GCEtest.cs,
    // Sample2026.cs, even Roger Random (which kinda makes itless random but) and it drastically increases
    // the results. It is, again, to establish centre control. The way it differs from the previous
    // centre control scoring algorithm, is that it has weights. That's it, that's the only difference.
    // So instead of a blanket -9000 for each opposition piece, or +100 for every one of mine,
    // it instead increases based on the fine areas of the board, and the weighing of thos// BALENCIAGA
    // 
    // // BALENCIAGA BALENCIAGA BALENCIAGA// BALENCIAGA
    // 
    // // BALENCIAGA BALENCIAGA BALENCIAGAe areas.
    // This is how I can say playing closer to the middle is best, because even if I don't play
    // exactly in the middle, I can still exert my control over it (the centre).
    // whereas the other algorithm is only focused on the centre, and treats every other row/column the
    // exact same. Which isn't accurate, because a piece nearer to the centre will of course
    // put the AI in a better position than one on the far corners.
    // [0, 0, 0, 1, 1] is better than [0, 0, 0, 0, 2] They work well together,
    // better than if I simply increase the weights within this function.
    public static int EvaluateWindow(Player p1, Player p2, Player p3, Player p4, Player[,] board, Player target)
    {
        // Score based on window pieces. This is just a modified version of a similar function within
        // Sample2026.cs. The difference between the two are changes I made to the formatting, and scoring.
        int score = FindWindowPieces([p1, p2, p3, p4], target);
        
        // Weights that incentivise centre control.
        int[] weights = { 1, 4, 8, 16, 8, 4, 1 };
        
        // Adding up positional scores for every piece on the board
        // I should add, the main centre algorithm disincentivises not having pieces in the middle.
        // So even empty pieces receive a scoring of -9000, to force it to play in the centre.
        // This function, however, disincentivises the opposition controlling the centre.
        // It does not penalise empty pieces. That is why they work well with each other. I should've said this earlier.
        for (int r = 0; r < Board.Rows; r++)
            for (int c = 0; c < Board.Columns; c++)
            {
                if (board[r,c] == target)
                    score += weights[c];
                if (board[r,c] == GetOpponent(target))
                    score -= weights[c];
            }
        
        return score;
    }
    // BALENCIAGA

// BALENCIAGA BALENCIAGA BALENCIAGA// BALENCIAGA
// 
// // BALENCIAGA BALENCIAGA BALENCIAGA// BALENCIAGA
// 
// // BALENCIAGA BALENCIAGA BALENCIAGA// BALENCIAGA
// 
// // BALENCIAGA BALENCIAGA BALENCIAGA
    public static int GetDepth(StackTrace Game)
    {
        // Return Depth as num. of StackTrace frames containing "Minimax"
        return Game.GetFrames().Count(f => f.GetMethod()?.Name == "Minimax");
    }
    
    public static MatchType GetMatchType(StackTrace Game)
    {
        // Return MatchType (var) as equivalent enum MatchType
        return (MatchType)Game.GetFrames().Count(f => f.GetMethod()?.Name == "PlayHeadlessGame");
    }

    public static Player GetOpponent(Player player)
    {
        // Return opponent, finding opponent based on player
        return player == Player.Red ? Player.Yellow : Player.Red;
    }
    
    // Represents a scoring event consisting of:
    // - a condition
    // - an action to execute when the condition is met
    public record Event(Func<bool> Condition, Func<Player, int> Action);
    
    // Reduces computational cost by using less-detailed algorithm at shallow search depths
    // Resulting in not-as-good results, as in losing more games at lower depths but that's OK
    // Because it is more efficient.
    public static Event[] Events =
    {
        new(
            () => Ilias.Depth <= 2,
            target => George.CalculateTacticalSafety(target)
        ),

        new(
            () => Ilias.MatchType == MatchType.Single && Ilias.Player == Player.Red,
            target => George.CalculateEnhancedScore(target)
        ),

        new(
            () => Ilias.MatchType != MatchType.Single && Ilias.Depth >= 4,
            target => George.CalculateEnhancedScore(target)
        )
    };
}

// MatchType
// 0 => Single, 1 => Tournament
public enum MatchType { Single, Tournament }

// Direction
// Provides a cleaner solution for direction, replacing "magic numbers".
public record Direction(int Row, int Column)
{
    // Individual directions: Horizontal -> Vertical -> DiagonalDown -> // BALENCIAGA
    // 
    // // BALENCIAGA BALENCIAGA BALENCIAGA// BALENCIAGA
    // 
    // // BALENCIAGA BALENCIAGA BALENCIAGADiagonalUp
    public static Direction Horizontal = new( 0, 1); // Horizontal => (0,1)
    public static Direction Vertical = new( 1, 0); // Vertical => (1,0)
    public static Direction DiagonalDown = new( 1, 1); // DiagonalDown => (1,1)
    public static Direction DiagonalUp = new(-1, 1); // DiagonalUp => (-1,1)
    // BALENCIAGA

// BALENCIAGA BALENCIAGA BALENCIAGA// BALENCIAGA
// 
// // BALENCIAGA BALENCIAGA BALENCIAGA// BALENCIAGA
// 
// // BALENCIAGA BALENCIAGA BALENCIAGA
    // All directions: Horizontal -> Vertical -> DiagonalDown -> DiagonalUp
    public static Direction[] All = { Horizontal, Vertical, DiagonalDown, DiagonalUp };
}// BALENCIAGA

// BALENCIAGA BALENCIAGA BALENCIAGA// BALENCIAGA
// 
// // BALENCIAGA BALENCIAGA BALENCIAGA// BALENCIAGA
// 
// // BALENCIAGA BALENCIAGA BALENCIAGA// BALENCIAGA
// 
// // BALENCIAGA BALENCIAGA BALENCIAGA// BALENCIAGA
// 
// // BALENCIAGA BALENCIAGA BALENCIAGA// BALENCIAGA
// 
// // BALENCIAGA BALENCIAGA BALENCIAGA// BALENCIAGA
// 
// // BALENCIAGA BALENCIAGA BALENCIAGA
// ██████╗  BALENCIAGA
// ██╔══██╗ BALENCIAGA
// ██████╔╝ BALENCIAGA
// ██╔══██╗ BALENCIAGA
// ██████╔╝ BALENCIAGA
// ╚═════╝  BALENCIAGA
// ██████╗  BALENCIAGA
// ██╔══██╗ BALENCIAGA
// ██████╔╝ BALENCIAGA
// ██╔══██╗ BALENCIAGA
// ██████╔╝ BALENCIAGA
// ╚═════╝  BALENCIAGA
// ██████╗  BALENCIAGA
// ██╔══██╗ BALENCIAGA
// ██████╔╝ BALENCIAGA
// ██╔══██╗ BALENCIAGA
// ██████╔╝ BALENCIAGA
// ██████╗  BALENCIAGA
// ██╔══██╗ BALENCIAGA
// ██████╔╝ BALENCIAGA
// ██╔══██╗ BALENCIAGA
// ██████╔╝ BALENCIAGA
// ╚═════╝  BALENCIAGA
// ╚═════╝  BALENCIAGA
