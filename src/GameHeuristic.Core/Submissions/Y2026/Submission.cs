using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameHeuristic.Core.Submissions.Y2026
{
    public class ThomasH : IHeuristic
    {
        public string Name { get; set; } = "GCE2026";
        private Random _random = new Random();

        public double Evaluate(Player[,] board, Player player)
        {
            return 10.0d;
        }
    }
}
