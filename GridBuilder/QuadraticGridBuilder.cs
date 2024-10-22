﻿using DataStructures.Geometry;

namespace GridBuilder;

public class QuadraticGridBuilder
{
    private GridParameters? Parameters { get; set; }
    
    private Point[] _points = null!;
    private List<FiniteElement> _finiteElements = null!;
    private readonly List<double> _circleMaterials = new();
    private HashSet<int> _dirichletBoundaries = new();
    private HashSet<Edge> _neumannBoundaries = new();

    private HashSet<int> _leftBorderElements = new();
    private HashSet<int> _rightBorderElements = new();
    private HashSet<int> _bottomBorderElements = new();
    private HashSet<int> _topBorderElements = new();
    private HashSet<int> _rightTearBorderElements = new();
    private HashSet<int> _topTearBorderElements = new();
    
    public (Point[], List<FiniteElement>, HashSet<int>, HashSet<Edge>) BuildGrid(GridParameters parameters)
    {
        Parameters = parameters;
        if (Parameters == null)
            throw new ArgumentNullException(nameof(parameters), "Grid parameters cannot be null.");
        
        CreatePoints();
        GenerateCircleMaterials();
        CreateElements();
        AccountBoundaryConditions();
        
        return (_points, _finiteElements, _dirichletBoundaries, _neumannBoundaries);
    }

    private void CreatePoints()
    {
        int innerX = 2 * Parameters!.XInnerSplits + 1;
        int innerY = 2 * Parameters.YInnerSplits + 1;

        int circleSplits = innerX + innerY - 1;
        
        double[] x = new double[innerX + 2 * Parameters.CircleSplits];
        double[] y = new double[innerY + 2 * Parameters.CircleSplits];
        
        _points = new Point[innerX * innerY + 2 * circleSplits * Parameters.CircleSplits];

        double xPoint = Parameters.XInterval.LeftBorder;
        double hx = Math.Abs(Parameters.XCoefficient - 1.0) < 1e-14
            ? Parameters.XInterval.Length / Parameters.XInnerSplits
            : Parameters.XInterval.Length * (1.0 - Parameters.XCoefficient) /
              (1.0 - Math.Pow(Parameters.XCoefficient, Parameters.XInnerSplits));
        double xCirclePoint = Parameters.XInterval.RightBorder;
        double hxCircle = Math.Abs(Parameters.CircleCoefficient- 1.0) < 1e-14
            ? Parameters.Radius / Parameters.CircleSplits
            : Parameters.Radius * (1.0 - Parameters.CircleCoefficient) /
              (1.0 - Math.Pow(Parameters.CircleCoefficient, Parameters.CircleSplits));
        xCirclePoint += hxCircle / 2.0;
        
        for (int i = 0; i < 2 * Parameters.XInnerSplits; i++)
        {
            x[i++] = xPoint;
            xPoint += hx / 2.0;
            x[i] = xPoint;
            xPoint += hx / 2.0;
            hx *= Parameters.XCoefficient;
        }
        x[2 * Parameters.XInnerSplits] = xPoint;

        for (int i = 0; i < 2 * Parameters.CircleSplits; i++)
        {
            x[innerX + i++] = xCirclePoint;
            xCirclePoint += hxCircle / 2.0;
            hxCircle *= Parameters.CircleCoefficient;
            x[innerX + i] = xCirclePoint;
            xCirclePoint += hxCircle / 2.0;
        }
        
        double yPoint = Parameters.YInterval.LeftBorder;
        double hy = Math.Abs(Parameters.YCoefficient - 1.0) < 1e-14
            ? Parameters.YInterval.Length / Parameters.YInnerSplits
            : Parameters.YInterval.Length * (1.0 - Parameters.YCoefficient) /
              (1.0 - Math.Pow(Parameters.YCoefficient, Parameters.YInnerSplits));
        double yCirclePoint = Parameters.YInterval.RightBorder;
        double hyCircle = Math.Abs(Parameters.CircleCoefficient- 1.0) < 1e-14
            ? Parameters.Radius / Parameters.CircleSplits
            : Parameters.Radius * (1.0 - Parameters.CircleCoefficient) /
              (1.0 - Math.Pow(Parameters.CircleCoefficient, Parameters.CircleSplits));
        yCirclePoint += hyCircle / 2.0;
        
        for (int i = 0; i < 2 * Parameters.YInnerSplits; i++)
        {
            y[i++] = yPoint;
            yPoint += hy / 2.0;
            y[i] = yPoint;
            yPoint += hy / 2.0;
            hy *= Parameters.YCoefficient;
        }
        y[2 * Parameters.YInnerSplits] = yPoint;

        for (int i = 0; i < 2 * Parameters.CircleSplits; i++)
        {
            y[innerY + i++] = yCirclePoint;
            yCirclePoint += hyCircle / 2.0;
            hyCircle *= Parameters.CircleCoefficient;
            y[innerY + i] = yCirclePoint;
            yCirclePoint += hyCircle / 2.0;
        }

        int iPoint = 0;
        // Inner scope
        for (int i = 0; i < innerY; i++)
        {
            for (int j = 0; j < innerX; j++)
            {
                _points[iPoint++] = new(x[j], y[i]);
            }
        }
        
        // Circle scope
        double theta = Math.PI / 2 / (Parameters.XInnerSplits + Parameters.YInnerSplits);
        for (int i = 0; i < 2 * Parameters.CircleSplits; i++)
        {
            double radius = x[innerX + i];

            for (int j = 0; j < innerX + innerY - 1; j++)
            {
                var angle = j * theta / 2.0;
                _points[iPoint++] = new(radius * Math.Cos(angle), radius * Math.Sin(angle));
            }
        }
    }

    private void GenerateCircleMaterials()
    {
        int innerX = Parameters!.XInnerSplits + 1;
        int innerY = Parameters.YInnerSplits + 1;
        
        double theta = Math.PI / 2  / (innerX + innerY - 2);
        for (int i = 1; i < innerX + innerY - 1; i++)
        {
            var angle = i * theta;
            var degrees = angle * 180 / Math.PI;

            for (int j = 0; j < Parameters.CircleMaterials.Count; j++)
            {
                if (Parameters.CircleMaterials[j].Degrees - degrees >= 1e-14)
                {
                    _circleMaterials.Add(Parameters.CircleMaterials[j].Material);
                    break;
                }
            }
        }
    }
    
    private void CreateElements()
    {
        int quadriteral = 9;
        _finiteElements = new List<FiniteElement>();
        Span<int> nodes = stackalloc int[quadriteral];
        
        int innerX = 2 * Parameters!.XInnerSplits + 1;
        int innerY = 2 * Parameters.YInnerSplits + 1;
        int innerSkip = innerX * innerY;
        int circleRow = innerX + innerY - 1;
        int circleXSkip = 2 * Parameters.XInnerSplits;

        // Inner scope
        for (int i = 0; i < Parameters.XInnerSplits; i++)
        {
            for (int j = 0; j < Parameters.YInnerSplits; j++)
            {
                nodes[0] = 2 * j + 2 * innerX * i;
                nodes[1] = 2 * j + 2 * innerX * i + 1;
                nodes[2] = 2 * j + 2 * innerX * i + 2;
                
                nodes[3] = 2 * j + 2 * innerX * i + innerX;
                nodes[4] = 2 * j + 2 * innerX * i + innerX + 1;
                nodes[5] = 2 * j + 2 * innerX * i + innerX + 2;
                
                nodes[6] = 2 * j + 2 * innerX * i + 2 * innerX;
                nodes[7] = 2 * j + 2 * innerX * i + 2 * innerX + 1;
                nodes[8] = 2 * j + 2 * innerX * i + 2 * innerX + 2;
                
                _finiteElements.Add(new FiniteElement(nodes.ToArray(), Parameters.Material, FiniteElementType.Quadratic));

                if (i == 0)
                {
                    _bottomBorderElements.Add(_finiteElements.Count - 1);
                }
                if (j == 0)
                {
                    _leftBorderElements.Add(_finiteElements.Count - 1);
                }
            }
        }

        int materialCounter = 0;
        
        // Inner vertical with circle scopes
        for (int i = 0; i < Parameters.XInnerSplits; i++)
        {
            nodes[0] = innerX + innerX * 2 * i - 1;
            nodes[1] = innerSkip + 2 * i;
            nodes[2] = innerSkip + circleRow + 2 * i;
            
            nodes[3] = innerX + innerX * 2 * i - 1 + innerX;
            nodes[4] = innerSkip + 2 * i + 1;
            nodes[5] = innerSkip + circleRow + 2 * i + 1;
            
            nodes[6] = innerX + innerX * 2 * i - 1 + 2 * innerX;
            nodes[7] = innerSkip + 2 * i + 2;
            nodes[8] = innerSkip + circleRow + 2 * i + 2;
            
            _finiteElements.Add(new FiniteElement(nodes.ToArray(), _circleMaterials[materialCounter++], FiniteElementType.Quadratic));

            if (i == 0)
            {
                _bottomBorderElements.Add(_finiteElements.Count - 1);
            }
        }
        
        // Inner horizontal with circle scopes
        for (int i = Parameters.YInnerSplits; i > 0; i--)
        {
            nodes[0] = innerSkip - (innerY - 2 * i) - 2;
            nodes[1] = innerSkip - (innerY - 2 * i) - 1;
            nodes[2] = innerSkip - (innerY - 2 * i);

            nodes[3] = innerSkip + circleXSkip + 2 + (innerY - 1 - 2 * i);
            nodes[4] = innerSkip + circleXSkip + 1 + (innerY - 1 - 2 * i);
            nodes[5] = innerSkip + circleXSkip + (innerY - 1 - 2 * i);

            nodes[6] = innerSkip + circleRow + circleXSkip + 2 + (innerY - 1 - 2 * i);
            nodes[7] = innerSkip + circleRow + circleXSkip + 1 + (innerY - 1 - 2 * i);
            nodes[8] = innerSkip + circleRow + circleXSkip + (innerY - 1 - 2 * i);
            
            _finiteElements.Add(new FiniteElement(nodes.ToArray(), _circleMaterials[materialCounter++], FiniteElementType.Quadratic));
        }
        _leftBorderElements.Add(_finiteElements.Count - 1);

        // Circle scope
        int depth = Parameters.CircleSplits - 1;
        for (int i = 0; i < Parameters.CircleSplits - 1; i++)
        {
            materialCounter = 0;
            for (int j = 0; j < (innerX + innerY - 2) / 2; j++)
            {
                // if (depth <= Parameters.CircleTear.Depth)
                // {
                //     if (j >= Parameters.CircleTear.Offset &&
                //         j <= Parameters.CircleTear.Offset + Parameters.CircleTear.Split - 1)
                //     {
                //         materialCounter++;
                //         continue;
                //     }
                // }

                
                if (j < (innerX + innerY - 2) / 4)
                {
                    int firstPoint = innerSkip + circleRow + 2 * i * circleRow + 2 * j;
                
                    nodes[0] = firstPoint;
                    nodes[1] = firstPoint + circleRow;
                    nodes[2] = firstPoint + 2 * circleRow;
                    
                    nodes[3] = firstPoint + 1;
                    nodes[4] = firstPoint + circleRow + 1;
                    nodes[5] = firstPoint + 2 * circleRow + 1;
                    
                    nodes[6] = firstPoint + 2;
                    nodes[7] = firstPoint + circleRow + 2;
                    nodes[8] = firstPoint + 2 * circleRow + 2;
                }
                else
                {
                    int firstPoint = innerSkip + circleRow + 2 * i * circleRow + 2 * j + 2;
                    
                    nodes[0] = firstPoint;
                    nodes[1] = firstPoint - 1;
                    nodes[2] = firstPoint - 2;
                    
                    nodes[3] = firstPoint + circleRow;
                    nodes[4] = firstPoint + circleRow - 1;
                    nodes[5] = firstPoint + circleRow - 2;
                    
                    nodes[6] = firstPoint + 2 * circleRow;
                    nodes[7] = firstPoint + 2 * circleRow - 1;
                    nodes[8] = firstPoint + 2 * circleRow - 2;
                }
                
                _finiteElements.Add(new FiniteElement(nodes.ToArray(), _circleMaterials[materialCounter++], FiniteElementType.Quadratic));

                if (j == 0)
                {
                    _bottomBorderElements.Add(_finiteElements.Count - 1);
                }

                if (Parameters.XInnerSplits % (j + 1) == 0)
                {
                    _leftBorderElements.Add(_finiteElements.Count - 1);
                }

                if (j < (innerX + innerY - 2) / 4 && i == Parameters.CircleSplits - 2)
                {
                    _rightBorderElements.Add(_finiteElements.Count - 1);
                }
                else if (j >= (innerX + innerY - 2) / 4 && i == Parameters.CircleSplits - 2)
                {
                    _topBorderElements.Add(_finiteElements.Count - 1);
                }
                
                // if (depth <= Parameters.CircleTear.Depth && j == Parameters.CircleTear.Offset - 1)
                // {
                //     _topTearBorderElements.Add(_finiteElements.Count - 1);
                // }
                // else if (depth <= Parameters.CircleTear.Depth &&
                //          j == Parameters.CircleTear.Offset + Parameters.CircleTear.Split)
                // {
                //     _rightTearBorderElements.Add(_finiteElements.Count - 1);
                // }
                //
                // if (depth - Parameters.CircleTear.Depth == 1 && j < (innerX + innerY - 2) / 2 &&
                //     j >= Parameters.CircleTear.Offset &&
                //     j <= Parameters.CircleTear.Offset + Parameters.CircleTear.Split - 1)
                // {
                //     _rightTearBorderElements.Add(_finiteElements.Count - 1);
                // }
                // else if (depth - Parameters.CircleTear.Depth == 1 && j >= (innerX + innerY - 2) / 2 &&
                //     j >= Parameters.CircleTear.Offset &&
                //     j <= Parameters.CircleTear.Offset + Parameters.CircleTear.Split - 1)
                // {
                //     _topTearBorderElements.Add(_finiteElements.Count - 1);
                // }
            }

            depth--;
        }
    }

    private void AccountBoundaryConditions()
    {
        foreach (var element in _leftBorderElements)
        {
            switch (Parameters!.LeftBorder)
            {
                case 1:
                    _dirichletBoundaries.Add(_finiteElements[element].Nodes[0]);
                    _dirichletBoundaries.Add(_finiteElements[element].Nodes[2]);
                    break;
                case 2:
                    _neumannBoundaries.Add(new Edge(_finiteElements[element].Nodes[0], _finiteElements[element].Nodes[2]));
                    break;
            }
        }

        foreach (var element in _bottomBorderElements)
        {
            switch (Parameters!.BottomBorder)
            {
                case 1:
                    _dirichletBoundaries.Add(_finiteElements[element].Nodes[0]);
                    _dirichletBoundaries.Add(_finiteElements[element].Nodes[1]);
                    break;
                case 2:
                    _neumannBoundaries.Add(new Edge(_finiteElements[element].Nodes[0], _finiteElements[element].Nodes[1]));
                    break;
            }
        }
        
        foreach (var element in _topBorderElements)
        {
            switch (Parameters!.CircleBorder)
            {
                case 1:
                    _dirichletBoundaries.Add(_finiteElements[element].Nodes[2]);
                    _dirichletBoundaries.Add(_finiteElements[element].Nodes[3]);
                    break;
                case 2:
                    _neumannBoundaries.Add(new Edge(_finiteElements[element].Nodes[2], _finiteElements[element].Nodes[3]));
                    break;
            }
        }
        
        foreach (var element in _rightBorderElements)
        {
            switch (Parameters!.CircleBorder)
            {
                case 1:
                    _dirichletBoundaries.Add(_finiteElements[element].Nodes[1]);
                    _dirichletBoundaries.Add(_finiteElements[element].Nodes[3]);
                    break;
                case 2:
                    _neumannBoundaries.Add(new Edge(_finiteElements[element].Nodes[1], _finiteElements[element].Nodes[3]));
                    break;
            }
        }

        foreach (var element in _topTearBorderElements)
        {
            switch (Parameters!.CircleTearBorder)
            {
                case 1:
                    _dirichletBoundaries.Add(_finiteElements[element].Nodes[2]);
                    _dirichletBoundaries.Add(_finiteElements[element].Nodes[3]);
                    break;
                case 2:
                    _neumannBoundaries.Add(new Edge(_finiteElements[element].Nodes[2], _finiteElements[element].Nodes[3]));
                    break;
            }
        }
        
        foreach (var element in _rightTearBorderElements)
        {
            switch (Parameters!.CircleTearBorder)
            {
                case 1:
                    _dirichletBoundaries.Add(_finiteElements[element].Nodes[1]);
                    _dirichletBoundaries.Add(_finiteElements[element].Nodes[3]);
                    break;
                case 2:
                    _neumannBoundaries.Add(new Edge(_finiteElements[element].Nodes[1], _finiteElements[element].Nodes[3]));
                    break;
            }
        }
    }
}