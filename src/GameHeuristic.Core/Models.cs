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
