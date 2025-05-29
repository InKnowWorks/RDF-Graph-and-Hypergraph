using System;
using System.Collections.Generic;
using Trinity;
using Trinity.Storage;
using RUB.TS.IG.GEAppServices.RPC.Protocols;

namespace SKOSHyperGraphExample
{
    public class BFS
    {
        public static List<long> BreadthFirstSearch(long startConceptId, Func<SKOSConcept_Accessor, bool> goalPredicate)
        {
            HashSet<long> visited = new HashSet<long>();
            Queue<long> queue = new Queue<long>();
            List<long> path = new List<long>();

            queue.Enqueue(startConceptId);

            while (queue.Count > 0)
            {
                long currentId = queue.Dequeue();
                visited.Add(currentId);

                using (SKOSConcept_Accessor currentConcept = Global.LocalStorage.UseSKOSConcept(currentId))
                {
                    if (currentConcept != null)
                    {
                        path.Add(currentId);

                        if (goalPredicate(currentConcept))
                        {
                            return path;
                        }

                        foreach (var hyperEdgeId in currentConcept.HyperEdges)
                        {
                            using (SKOSHyperEdge_Accessor hyperEdge = Global.LocalStorage.UseSKOSHyperEdge(hyperEdgeId))
                            {
                                if (hyperEdge != null)
                                {
                                    foreach (var relatedConceptId in hyperEdge.RelatedConcepts)
                                    {
                                        if (!visited.Contains(relatedConceptId))
                                        {
                                            queue.Enqueue(relatedConceptId);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                path.RemoveAt(path.Count - 1);
            }

            return null; // No path found
        }
    }
}
