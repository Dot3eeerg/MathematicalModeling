﻿using DataStructures.Geometry;

namespace GridBuilder;

public class GridBuilder
{
    private GridParameters? Parameters { get; set; }

    private Point[] _points = null!;
    private List<FiniteElement> _finiteElements = null!;
    private List<double> _circleMaterials = new List<double>();
    
    public (Point[], List<FiniteElement>) BuildGrid(GridParameters parameters)
    {
        Parameters = parameters;
        if (Parameters == null)
            throw new ArgumentNullException(nameof(parameters), "Grid parameters cannot be null.");
        
        CreatePoints();
        GenerateCircleMaterials();
        CreateElements();
        AccountBoundaryConditions();
        
        return (_points, _finiteElements);
    }

    private void CreatePoints()
    {
        int innerX = Parameters!.XInnerSplits + 1;
        int innerY = Parameters.YInnerSplits + 1;
        
        int circleSplits = Parameters.XInnerSplits + Parameters.YInnerSplits + 1;
        
        double[] x = new double[innerX + Parameters.CircleSplits];
        double[] y = new double[innerY + Parameters.CircleSplits];
        
        _points = new Point[innerX * innerY + circleSplits * Parameters.CircleSplits];

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
        xCirclePoint += hxCircle;
        hxCircle *= Parameters.CircleCoefficient;
        
        for (int i = 0; i < innerX; i++)
        {
            x[i] = xPoint;
            xPoint += hx;
            hx *= Parameters.XCoefficient;
        }

        for (int i = 0; i < Parameters.CircleSplits; i++)
        {
            x[innerX + i] = xCirclePoint;
            xCirclePoint += hxCircle;
            hxCircle *= Parameters.CircleCoefficient;
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
        yCirclePoint += hyCircle;
        hyCircle *= Parameters.CircleCoefficient;
        
        for (int i = 0; i < innerX; i++)
        {
            y[i] = yPoint;
            yPoint += hy;
            hy *= Parameters.YCoefficient;
        }

        for (int i = 0; i < Parameters.CircleSplits; i++)
        {
            y[innerY + i] = yCirclePoint;
            yCirclePoint += hyCircle;
            hyCircle *= Parameters.CircleCoefficient;
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
        double theta = Math.PI / 2  / (innerX + innerY - 2);
        for (int i = 0; i < Parameters.CircleSplits; i++)
        {
            double radius = x[innerX + i];

            for (int j = 0; j < innerX + innerY - 1; j++)
            {
                var angle = j * theta;
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
        int quadriteral = 4;
        _finiteElements = new List<FiniteElement>();
        Span<int> nodes = stackalloc int[quadriteral];
        
        int innerX = Parameters!.XInnerSplits + 1;
        int innerY = Parameters.YInnerSplits + 1;

        // Inner scope
        for (int i = 0; i < innerY - 1; i++)
        {
            for (int j = 0; j < innerX - 1; j++)
            {
                nodes[0] = j + innerX * i;
                nodes[1] = j + innerX * i + 1;
                nodes[2] = j + innerX * i + innerX;
                nodes[3] = j + innerX * i + innerX + 1;

                _finiteElements.Add(new FiniteElement(nodes.ToArray(), Parameters.Material));
            }
        }

        int materialCounter = 0;
        // Inner vertical with circle scopes
        for (int i = 0; i < innerX - 1; i++)
        {
            nodes[0] = innerX + innerX * i - 1;
            nodes[1] = innerX * innerY + i;
            nodes[2] = innerX * i + 2 * innerX - 1;
            nodes[3] = innerX * innerY + i + 1;
            
            _finiteElements.Add(new FiniteElement(nodes.ToArray(), _circleMaterials[materialCounter++]));
        }
        
        // Inner horizontal with circle scopes
        for (int i = innerY - 1; i > 0; i--)
        {
            nodes[0] = innerX * innerY - (innerY - i) - 1;
            nodes[1] = innerX * innerY - (innerY - i);
            nodes[2] = innerX * innerY + innerX + (innerY - i - 1);
            nodes[3] = innerX * innerY + innerX + (innerY - i - 1) - 1;
            
            _finiteElements.Add(new FiniteElement(nodes.ToArray(), _circleMaterials[materialCounter++]));
        }

        int depth = Parameters.CircleSplits - 1;
        int skipToCircle = innerX * innerY;
        int innerCircle = innerX + innerY - 1;
        // Circle scope
        for (int i = 0; i < Parameters.CircleSplits - 1; i++)
        {
            materialCounter = 0;
            for (int j = 0; j < innerX + innerY - 2; j++)
            {
                nodes[0] = skipToCircle + j + i * innerCircle;
                nodes[1] = skipToCircle + j + i * innerCircle + 1;
                nodes[2] = skipToCircle + j + i * innerCircle + innerCircle;
                nodes[3] = skipToCircle + j + i * innerCircle + 1 + innerCircle;
                
                _finiteElements.Add(new FiniteElement(nodes.ToArray(), _circleMaterials[materialCounter++]));
            }
        }
    }

    private void AccountBoundaryConditions()
    {
        
    }
}