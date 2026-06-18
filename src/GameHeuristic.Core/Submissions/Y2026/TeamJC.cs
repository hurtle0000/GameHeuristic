namespace GameHeuristic.Core.Submissions.Y2026;

/// <summary>
/// A sample student submission for the class of 2026.
/// This bot greedily tries to build its own lines of 2 and 3, completely ignoring the opponent's moves.
/// </summary>
public class TeamJC : IHeuristic
{
    public string Name => "TeamJC";

    public double Evaluate(Player[,] board, Player player)
    {
        return 10.0;
    }
}


