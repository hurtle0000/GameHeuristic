using System;
using GameHeuristic.Core;

namespace GameHeuristic.Core.Submissions.Y2026;

/// <summary>
/// A sample student submission for the class of 2026.
/// This bot greedily tries to build its own lines of 2 and 3, completely ignoring the opponent's moves.
/// </summary>
public class mathewCuthburt : IHeuristic
{
    //Go first. If your opponent disagrees, find a differnet opponent.
    //2. Play the first move in centre column
    //3. Play the rest of the game perfectly
    public string Name => "Mathew Cuthburt";

    //private struct Evaluation
    //{
    //}
    //Too much time was dedicated to this. I still havent started my NEA design :)
    public double Evaluate(Player[,] b, Player p)
    {
        Player opp;
        string pStr = p.ToString();
        if (pStr == "Red")
        {
            opp = Player.Yellow;
        }
        else
        {
            opp = Player.Red;
        }

        //int d = 100;

        double s = 0.0;
        int h = b.GetLength(0);
        int w = b.GetLength(1);
        int[,] matrix =
        {
            { 3, 4, 5, 7, 5, 4, 3 },
            { 4, 6, 8, 10, 8, 6, 4 },
            { 5, 8, 11, 13, 11, 8, 5 },
            { 5, 8, 11, 13, 11, 8, 5 }, //Centre wight
            { 4, 6, 8, 10, 8, 6, 4 },
            { 3, 4, 5, 7, 5, 4, 3 }
        };

        //Score check
        for (int r = h - 1; r >= 0; r--)
        {
            for (int c = 0; c < w; c++)
            {
                string cellValue = b[r, c].ToString();
                if (cellValue == p.ToString())
                {
                    s = s + (double)matrix[r, c];
                }
                else
                {
                    if (cellValue == opp.ToString())
                    {
                        s = s - (double)matrix[r, c];
                    }
                }
            }
        }

        /*
        for (int c = 0; c < w; c++)
        {
            for (int r = 0; r < h; r++)
            {
                if (b[r, c] == Player.None && b[r + 1, c] == p)
                {
                    if (b[r, c] == o)
                    {
                        s -= 9999.0;
                    }
                }
            }
        } */

        //COMBINATIONS



        //67








        //HORIZONATAL
        for (int r = 0; r < h; r++)
        {
            for (int c = 0; c <= w - 4; c++)
            {
                s += secondMethodineeded(b[r, c], b[r, c + 1], b[r, c + 2], b[r, c + 3], p, opp);
            }
        }

        //VERTICLE
        for (int c = 0; c < w; c++)
        {
            for (int r = 0; r <= h - 4; r++)
            {
                s += secondMethodineeded(b[r, c], b[r + 1, c], b[r + 2, c], b[r + 3, c], p, opp);
            }
        }

        //      for (int x = 0; x < h - 2; x++)
        //    {
        //      for (int y = 0; y < w - 2; y++)
        //      {
        //         int myCounters = 0;
        //        if (b[x, y] == p) myCounters++;
        //      if (b[x + 1, y + 1] == p) myCounters++;
        //      if (b[x + 2, y] == p) myCounters++;
        //
        //              if (myCounters == 3)
        //            {
        //              s += 400.0;
        //        }
        //    }
        // }

        //DOWNRIGHT
        for (int r = 0; r <= h - 4; r++)
        {
            for (int c = 0; c <= w - 4; c++)
            {
                s += secondMethodineeded(b[r, c], b[r + 1, c + 1], b[r + 2, c + 2], b[r + 3, c + 3], p, opp);
            }
        }

        //upright but dont work atm
        for (int r = 3; r < h; r++)
        {
            for (int c = 0; c <= w - 4; c++)
            {
                s += secondMethodineeded(b[r, c], b[r - 1, c + 1], b[r - 2, c + 2], b[r - 3, c + 3], p, opp);
            }
        }
        return s;
    }


    private double secondMethodineeded(Player p1, Player p2, Player p3, Player p4, Player p, Player o)
    {
        int[] counts = new int[2] { 0, 0 };

        if (p1 == p) counts[0]++; else if (p1 == o) counts[1]++;
        if (p2 == p) counts[0]++; else if (p2 == o) counts[1]++;
        if (p3 == p) counts[0]++; else if (p3 == o) counts[1]++;
        if (p4 == p) counts[0]++; else if (p4 == o) counts[1]++;

        int pCount = counts[0];
        int oppCount = counts[1];

        int structuralHash = (pCount * 10) + oppCount;
        switch (structuralHash)
        {
            case 40:
                return 50001.0;
            case 4:
                return -50000.0;
            case 30:
                return 310.0;
            case 3:
                return -295.0;
            case 20:
                return 40.0;
            case 2:
                return -45.0;
            default:
                return (double)(0 * 1);
        }

    }

    /*
     * private double V(Player p1, Player p2, Player p3, Player p4, Player p, Player o)
    {
        int pCount = 0; 
        int oppCount = 0;

        

        if (pCount == 4) return 50001.0;
        if (oppCount == 4) return -50000.0;

        if (pCount == 3 && oppCount == 0) return 250.0;
        if (oppCount == 3 && pCount == 0) return -300.0;

        if (pCount == 2 && oppCount == 0) return 40.0;
        if (oppCount == 2 && pCount == 0) return -45.0;

        return 0.0; */
}
