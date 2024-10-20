﻿using DataStructures.Geometry;

namespace GridBuilder;

public class Grid
{
    private readonly GridBuilder _builder = new();
    
    public IReadOnlyList<Point>? Points { get; private set; }
    public IReadOnlyList<FiniteElement>? FiniteElements { get; private set; }
    public IReadOnlySet<int>? DirichletNodes { get; private set; }
    public IReadOnlySet<Edge>? NeumannEdges { get; private set; }

    public void Build(GridParameters parameters)
    {
        (Points, FiniteElements, DirichletNodes, NeumannEdges) = _builder.BuildGrid(parameters);
    }

    public void SaveGrid(string folder)
    {
        var sw = new StreamWriter($"{folder}/points");
        foreach (var point in Points!)
        {
            sw.WriteLine(point.ToString());
        }
        sw.Close();
        
        sw = new StreamWriter($"{folder}/finite_elements");
        foreach (var element in FiniteElements!)
        {
            sw.WriteLine(element.ToString());
        }
        sw.Close();
        
        sw = new StreamWriter($"{folder}/dirichlet");
        foreach (var node in DirichletNodes!)
        {
            sw.WriteLine(node.ToString());
        }
        sw.Close();
        
        sw = new StreamWriter($"{folder}/neumann");
        foreach (var edge in NeumannEdges!)
        {
            sw.WriteLine(edge.ToString());
        }
        sw.Close();
    }
}