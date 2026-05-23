namespace GameHeuristic.Core;

public class Board
{
    public const int Rows = 6;
    public const int Columns = 7;
    private readonly Player[,] _grid;

    public Board()
    {
        _grid = new Player[Rows, Columns];
    }

    private Board(Player[,] grid)
    {
        _grid = (Player[,])grid.Clone();
    }

    public Player GetPiece(int row, int col) => _grid[row, col];

    public Player[,] GetGridCopy() => (Player[,])_grid.Clone();

    public bool CanMakeMove(int col)
    {
        if (col < 0 || col >= Columns) return false;
        return _grid[0, col] == Player.None;
    }

    public bool MakeMove(int col, Player player)
    {
        if (!CanMakeMove(col)) return false;

        for (int r = Rows - 1; r >= 0; r--)
        {
            if (_grid[r, col] == Player.None)
            {
                _grid[r, col] = player;
                return true;
            }
        }
        return false;
    }

    public Board Clone() => new Board(_grid);

    public GameState CheckGameState()
    {
        // Check horizontal
        for (int r = 0; r < Rows; r++)
        {
            for (int c = 0; c <= Columns - 4; c++)
            {
                Player p = _grid[r, c];
                if (p != Player.None && p == _grid[r, c + 1] && p == _grid[r, c + 2] && p == _grid[r, c + 3])
                    return p == Player.Red ? GameState.RedWin : GameState.YellowWin;
            }
        }

        // Check vertical
        for (int r = 0; r <= Rows - 4; r++)
        {
            for (int c = 0; c < Columns; c++)
            {
                Player p = _grid[r, c];
                if (p != Player.None && p == _grid[r + 1, c] && p == _grid[r + 2, c] && p == _grid[r + 3, c])
                    return p == Player.Red ? GameState.RedWin : GameState.YellowWin;
            }
        }

        // Check diagonal (down-right)
        for (int r = 0; r <= Rows - 4; r++)
        {
            for (int c = 0; c <= Columns - 4; c++)
            {
                Player p = _grid[r, c];
                if (p != Player.None && p == _grid[r + 1, c + 1] && p == _grid[r + 2, c + 2] && p == _grid[r + 3, c + 3])
                    return p == Player.Red ? GameState.RedWin : GameState.YellowWin;
            }
        }

        // Check diagonal (up-right)
        for (int r = 3; r < Rows; r++)
        {
            for (int c = 0; c <= Columns - 4; c++)
            {
                Player p = _grid[r, c];
                if (p != Player.None && p == _grid[r - 1, c + 1] && p == _grid[r - 2, c + 2] && p == _grid[r - 3, c + 3])
                    return p == Player.Red ? GameState.RedWin : GameState.YellowWin;
            }
        }

        // Check for draw
        bool full = true;
        for (int c = 0; c < Columns; c++)
        {
            if (_grid[0, c] == Player.None)
            {
                full = false;
                break;
            }
        }

        return full ? GameState.Draw : GameState.Ongoing;
    }
}
