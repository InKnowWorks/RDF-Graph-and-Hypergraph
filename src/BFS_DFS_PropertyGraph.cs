using System;
using System.Collections.Generic;

// Vertex and Edge classes remain unchanged from the previous example.

public class PropertyGraph
{
    public Dictionary<int, Vertex> Vertices { get; set; }
    public Dictionary<int, Edge> Edges { get; set; }

    // AddVertex and AddEdge methods remain unchanged from the previous example.

    public void BFS(int startVertexId)
    {
        if (!Vertices.ContainsKey(startVertexId))
        {
            Console.WriteLine("The specified start vertex does not exist.");
            return;
        }

        var visited = new HashSet<int>();
        var queue = new Queue<Vertex>();

        visited.Add(startVertexId);
        queue.Enqueue(Vertices[startVertexId]);

        while (queue.Count > 0)
        {
            Vertex current = queue.Dequeue();
            Console.WriteLine($"Visited vertex: {current.Id}");

            foreach (var edge in Edges.Values)
            {
                if (edge.Source.Id == current.Id && !visited.Contains(edge.Target.Id))
                {
                    visited.Add(edge.Target.Id);
                    queue.Enqueue(edge.Target);
                }
            }
        }
    }

    public void DFS(int startVertexId)
    {
        if (!Vertices.ContainsKey(startVertexId))
        {
            Console.WriteLine("The specified start vertex does not exist.");
            return;
        }

        var visited = new HashSet<int>();
        DFSUtil(startVertexId, visited);
    }

    private void DFSUtil(int vertexId, HashSet<int> visited)
    {
        visited.Add(vertexId);
        Console.WriteLine($"Visited vertex: {vertexId}");

        foreach (var edge in Edges.Values)
        {
            if (edge.Source.Id == vertexId && !visited.Contains(edge.Target.Id))
            {
                DFSUtil(edge.Target.Id, visited);
            }
        }
    }
}

public class Program
{
    public static void Main()
    {
        var graph = new PropertyGraph();

        // Add vertices and edges as shown in the previous example.

        Console.WriteLine("Breadth-First Search:");
        graph.BFS(1); // Replace '1' with the desired starting vertex ID.

        Console.WriteLine("Depth-First Search:");
        graph.DFS(1); // Replace '1' with the desired starting vertex ID.
    }
}
