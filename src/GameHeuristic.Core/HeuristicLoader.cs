using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GameHeuristic.Core;

public static class HeuristicLoader
{
    public static List<IHeuristic> LoadHeuristics()
    {
        List<IHeuristic> heuristics = new List<IHeuristic>();
        
        // Load from all loaded assemblies
        IEnumerable<Type> types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => typeof(IHeuristic).IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract);

        foreach (Type type in types)
        {
            try
            {
                if (Activator.CreateInstance(type) is IHeuristic heuristic)
                {
                    heuristics.Add(heuristic);
                }
            }
            catch
            {
                // Skip if couldn't instantiate
            }
        }

        return heuristics.OrderBy(h => h.Name).ToList();
    }
}
