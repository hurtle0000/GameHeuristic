namespace GameHeuristic.Core;

public enum Player
{
    None = 0,
    Red = 1,
    Yellow = 2
}

public enum GameState
{
    Ongoing,
    RedWin,
    YellowWin,
    Draw
}

public interface IHeuristic
{
    string Name { get; }
    double Evaluate(Player[,] board, Player player);
}

public class KnockoutMatch
{
    public string Player1 { get; set; } = string.Empty;
    public string Player2 { get; set; } = string.Empty;
    public string Winner { get; set; } = string.Empty;
    public int RoundIndex { get; set; }
    public int MatchIndex { get; set; }
}
