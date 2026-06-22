using System;
using GameHeuristic.Core;

namespace GameHeuristic.Core.Submissions.Baselines;

// A couple of example player heuristics for reference


// GCE test
public class IUA : IHeuristic
{
	public string Name { get; set; } = "Insane Unbeatable Algorithm (VIK+WILL)";
	private Random _random = new Random();

	public double Evaluate(Player[,] board, Player player)
	{
		return 10.0d;
	}
}