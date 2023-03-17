using System;
using System.Collections.Generic;

public class Vertex
{
    public int Id { get; set; }
    public Dictionary<string, object> Properties { get; set; }

    public Vertex(int id)
    {
        Id = id;
        Properties = new Dictionary<string, object>();
    }
}

public class Edge
{
    public int Id { get; set; }
    public Vertex Source { get; set; }
    public Vertex Target { get; set; }
    public Dictionary<string, object> Properties { get; set; }

    public Edge(int id, Vertex source, Vertex target)
    {
        Id = id;
        Source = source;
        Target = target;
        Properties = new Dictionary<string, object>();
    }
}

public class PropertyGraph
{
    public Dictionary<int, Vertex> Vertices { get; set; }
    public Dictionary<int, Edge> Edges { get; set; }

    public PropertyGraph()
    {
        Vertices = new Dictionary<int, Vertex>();
        Edges = new Dictionary<int, Edge>();
    }

    public void AddVertex(Vertex vertex)
    {
        if (!Vertices.ContainsKey(vertex.Id))
        {
            Vertices.Add(vertex.Id, vertex);
        }
    }

    public void AddEdge(Edge edge)
    {
        if (!Edges.ContainsKey(edge.Id))
        {
            Edges.Add(edge.Id, edge);
        }
    }
}

public class Program
{
    public static void Main()
    {
        var graph = new PropertyGraph();

        var vertex1 = new Vertex(1);
        vertex1.Properties.Add("name", "Alice");
        graph.AddVertex(vertex1);

        var vertex2 = new Vertex(2);
        vertex2.Properties.Add("name", "Bob");
        graph.AddVertex(vertex2);

        var edge1 = new Edge(1, vertex1, vertex2);
        edge1.Properties.Add("relationship", "friends");
        graph.AddEdge(edge1);

        Console.WriteLine("Vertices:");
        foreach (var vertex in graph.Vertices.Values)
        {
            Console.WriteLine($"ID: {vertex.Id}, Name: {vertex.Properties["name"]}");
        }

        Console.WriteLine("Edges:");
        foreach (var edge in graph.Edges.Values)
        {
            Console.WriteLine($"ID: {edge.Id}, Source: {edge.Source.Id}, Target: {edge.Target.Id}, Relationship: {edge.Properties["relationship"]}");
        }
    }
}
