using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GameHeuristic.Core;

public static class HeuristicLoader
{
    private const string NamespacePrefix = "GameHeuristic.Core.Submissions.";

    /// <summary>
    /// Scans the assembly to find all submission folders/sub-namespaces.
    /// E.g., returns ["Baselines", "Y2026"]
    /// </summary>
    public static List<string> GetAvailableGroups()
    {
        var groups = new HashSet<string>();
        
        IEnumerable<Type> types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => typeof(IHeuristic).IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract);

        foreach (var type in types)
        {
            string ns = type.Namespace ?? "";
            if (ns.StartsWith(NamespacePrefix))
            {
                string group = ns.Substring(NamespacePrefix.Length);
                // Get only the first segment of the namespace after Submissions.
                int dotIndex = group.IndexOf('.');
                if (dotIndex > 0)
                {
                    group = group.Substring(0, dotIndex);
                }

                if (!string.IsNullOrEmpty(group))
                {
                    groups.Add(group);
                }
            }
        }

        return groups.OrderBy(g => g).ToList();
    }

    /// <summary>
    /// Loads heuristics, optionally filtered by group namespace segment.
    /// E.g., "Baselines" only loads under GameHeuristic.Core.Submissions.Baselines.
    /// </summary>
    public static List<IHeuristic> LoadHeuristics(string group = "All")
    {
        List<IHeuristic> heuristics = new List<IHeuristic>();
        
        IEnumerable<Type> types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => typeof(IHeuristic).IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract);

        if (!string.IsNullOrEmpty(group) && group != "All")
        {
            string targetNamespacePrefix = $"{NamespacePrefix}{group}";
            types = types.Where(p => p.Namespace != null && 
                                    (p.Namespace == targetNamespacePrefix || p.Namespace.StartsWith(targetNamespacePrefix + ".")));
        }

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
