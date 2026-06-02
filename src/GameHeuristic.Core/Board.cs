using System.Collections.Generic;

namespace GameHeuristic.Core;

public class Board
{
    // The board is a 2D array with 0,0 as the "top-left" window and [5,6] as the "bottom-right"

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

    public Player GetPiece(int row, int col)
    {
        return _grid[row, col];
    }

    public Player[,] GetGridCopy()
    {
        return (Player[,])_grid.Clone();
    }

    public bool CanMakeMove(int col)
    {
        if (col < 0 || col >= Columns) return false;
        return _grid[0, col] == Player.None;
    }

    public int GetLandingRow(int col)
    {
        if (col < 0 || col >= Columns) return -1;
        for (int r = Rows - 1; r >= 0; r--)
        {
            if (_grid[r, col] == Player.None)
            {
                return r;
            }
        }
        return -1;
    }

    public bool MakeMove(int col, Player player)
    {
        int landingRow = GetLandingRow(col);
        if (landingRow == -1) return false;

        _grid[landingRow, col] = player;
        return true;
    }

    public Board Clone()
    {
        return new Board(_grid);
    }

    public List<(int Row, int Col)> GetWinningLine()
    {
        var line = new List<(int Row, int Col)>();

        // Check horizontal
        for (int r = 0; r < Rows; r++)
        {
            for (int c = 0; c <= Columns - 4; c++)
            {
                Player p = _grid[r, c];
                if (p != Player.None && p == _grid[r, c + 1] && p == _grid[r, c + 2] && p == _grid[r, c + 3])
                {
                    line.Add((r, c));
                    line.Add((r, c + 1));
                    line.Add((r, c + 2));
                    line.Add((r, c + 3));
                    return line;
                }
            }
        }

        // Check vertical
        for (int r = 0; r <= Rows - 4; r++)
        {
            for (int c = 0; c < Columns; c++)
            {
                Player p = _grid[r, c];
                if (p != Player.None && p == _grid[r + 1, c] && p == _grid[r + 2, c] && p == _grid[r + 3, c])
                {
                    line.Add((r, c));
                    line.Add((r + 1, c));
                    line.Add((r + 2, c));
                    line.Add((r + 3, c));
                    return line;
                }
            }
        }

        // Check diagonal (down-right)
        for (int r = 0; r <= Rows - 4; r++)
        {
            for (int c = 0; c <= Columns - 4; c++)
            {
                Player p = _grid[r, c];
                if (p != Player.None && p == _grid[r + 1, c + 1] && p == _grid[r + 2, c + 2] && p == _grid[r + 3, c + 3])
                {
                    line.Add((r, c));
                    line.Add((r + 1, c + 1));
                    line.Add((r + 2, c + 2));
                    line.Add((r + 3, c + 3));
                    return line;
                }
            }
        }

        // Check diagonal (up-right)
        for (int r = 3; r < Rows; r++)
        {
            for (int c = 0; c <= Columns - 4; c++)
            {
                Player p = _grid[r, c];
                if (p != Player.None && p == _grid[r - 1, c + 1] && p == _grid[r - 2, c + 2] && p == _grid[r - 3, c + 3])
                {
                    line.Add((r, c));
                    line.Add((r - 1, c + 1));
                    line.Add((r - 2, c + 2));
                    line.Add((r - 3, c + 3));
                    return line;
                }
            }
        }

        return line;
    }

    public GameState CheckGameState()
    {
        var winLine = GetWinningLine();
        if (winLine.Count > 0)
        {
            Player p = _grid[winLine[0].Row, winLine[0].Col];
            return p == Player.Red ? GameState.RedWin : GameState.YellowWin;
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
