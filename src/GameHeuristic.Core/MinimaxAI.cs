using System;

namespace GameHeuristic.Core;

public class MinimaxAI
{
    private readonly IHeuristic _heuristic;
    private readonly int _depth;

    public MinimaxAI(IHeuristic heuristic, int depth = 6)
    {
        _heuristic = heuristic;
        _depth = depth;
    }

    public int GetBestMove(Board board, Player player)
    {
        int bestMove = -1;
        double bestValue = double.NegativeInfinity;
        Player opponent = player == Player.Red ? Player.Yellow : Player.Red;

        for (int col = 0; col < Board.Columns; col++)
        {
            if (board.CanMakeMove(col))
            {
                Board nextBoard = board.Clone();
                nextBoard.MakeMove(col, player);
                double moveValue = Minimax(nextBoard, _depth - 1, false, double.NegativeInfinity, double.PositiveInfinity, player, opponent);

                if (moveValue > bestValue)
                {
                    bestValue = moveValue;
                    bestMove = col;
                }
            }
        }

        return bestMove;
    }

    private double Minimax(Board board, int depth, bool isMaximizing, double alpha, double beta, Player player, Player opponent)
    {
        GameState state = board.CheckGameState();
        if (state != GameState.Ongoing)
        {
            if (state == GameState.Draw) return 0;
            if ((state == GameState.RedWin && player == Player.Red) || (state == GameState.YellowWin && player == Player.Yellow))
                return 1000000 + depth; // Favor quicker wins
            return -1000000 - depth; // Favor delayed losses
        }

        if (depth == 0)
        {
            return _heuristic.Evaluate(board.GetGridCopy(), player);
        }

        if (isMaximizing)
        {
            double maxEval = double.NegativeInfinity;
            for (int col = 0; col < Board.Columns; col++)
            {
                if (board.CanMakeMove(col))
                {
                    Board nextBoard = board.Clone();
                    nextBoard.MakeMove(col, player);
                    double eval = Minimax(nextBoard, depth - 1, false, alpha, beta, player, opponent);
                    maxEval = Math.Max(maxEval, eval);
                    alpha = Math.Max(alpha, eval);
                    if (beta <= alpha) break;
                }
            }
            return maxEval;
        }
        else
        {
            double minEval = double.PositiveInfinity;
            for (int col = 0; col < Board.Columns; col++)
            {
                if (board.CanMakeMove(col))
                {
                    Board nextBoard = board.Clone();
                    nextBoard.MakeMove(col, opponent);
                    double eval = Minimax(nextBoard, depth - 1, true, alpha, beta, player, opponent);
                    minEval = Math.Min(minEval, eval);
                    beta = Math.Min(beta, eval);
                    if (beta <= alpha) break;
                }
            }
            return minEval;
        }
    }
}
