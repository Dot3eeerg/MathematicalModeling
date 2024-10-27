using DataStructures.Geometry;

namespace GridBuilder;

public class LinearGridBuilder
{
    private GridParameters? Parameters { get; set; }
    
    private Point[] _points = null!;
    private List<FiniteElement> _finiteElements = null!;
    private readonly List<double> _circleMaterials = new();
    private readonly HashSet<int> _dirichletBoundaries = new();
    private readonly HashSet<Edge> _neumannBoundaries = new();

    private readonly HashSet<int> _leftBorderElements = new();
    private readonly HashSet<int> _rightBorderElements = new();
    private readonly HashSet<int> _bottomBorderElements = new();
    private readonly HashSet<int> _topBorderElements = new();
    private readonly HashSet<int> _rightTearBorderElements = new();
    private readonly HashSet<int> _topTearBorderElements = new();
    
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
                
                _finiteElements.Add(new FiniteElement(nodes.ToArray(), Parameters.Material, FiniteElementType.Linear));

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
        for (int i = 0; i < innerX - 1; i++)
        {
            nodes[0] = innerX + innerX * i - 1;
            nodes[1] = innerX * innerY + i;
            nodes[2] = innerX * i + 2 * innerX - 1;
            nodes[3] = innerX * innerY + i + 1;
            
            _finiteElements.Add(new FiniteElement(nodes.ToArray(), _circleMaterials[materialCounter++], FiniteElementType.Linear));

            if (i == 0)
            {
                _bottomBorderElements.Add(_finiteElements.Count - 1);
            }
        }
        
        // Inner horizontal with circle scopes
        for (int i = innerY - 1; i > 0; i--)
        {
            nodes[0] = innerX * innerY - (innerY - i) - 1;
            nodes[1] = innerX * innerY - (innerY - i);
            nodes[2] = innerX * innerY + innerX + (innerY - i - 1);
            nodes[3] = innerX * innerY + innerX + (innerY - i - 1) - 1;
            
            _finiteElements.Add(new FiniteElement(nodes.ToArray(), _circleMaterials[materialCounter++], FiniteElementType.Linear));
        }
        _leftBorderElements.Add(_finiteElements.Count - 1);

        // Circle scope
        int depth = Parameters.CircleSplits - 1;
        int skipToCircle = innerX * innerY;
        int innerCircle = innerX + innerY - 1;
        for (int i = 0; i < Parameters.CircleSplits - 1; i++)
        {
            materialCounter = 0;
            for (int j = 0; j < innerX + innerY - 2; j++)
            {
                if (depth <= Parameters.CircleTear.Depth)
                {
                    if (j >= Parameters.CircleTear.Offset &&
                        j <= Parameters.CircleTear.Offset + Parameters.CircleTear.Split - 1)
                    {
                        materialCounter++;
                        continue;
                    }
                }

                if (j < (innerX + innerY - 2) / 2)
                {
                    nodes[0] = skipToCircle + j + i * innerCircle;
                    nodes[1] = skipToCircle + j + i * innerCircle + innerCircle;
                    nodes[2] = skipToCircle + j + i * innerCircle + 1;
                    nodes[3] = skipToCircle + j + i * innerCircle + 1 + innerCircle;
                }
                else
                {
                    nodes[0] = skipToCircle + j + i * innerCircle + 1;
                    nodes[1] = skipToCircle + j + i * innerCircle;
                    nodes[2] = skipToCircle + j + i * innerCircle + 1 + innerCircle;
                    nodes[3] = skipToCircle + j + i * innerCircle + innerCircle;
                }
                
                _finiteElements.Add(new FiniteElement(nodes.ToArray(), _circleMaterials[materialCounter++], FiniteElementType.Linear));

                if (j == 0)
                {
                    _bottomBorderElements.Add(_finiteElements.Count - 1);
                }

                if (j == innerX + innerY - 3)
                {
                    _leftBorderElements.Add(_finiteElements.Count - 1);
                }

                if (j < (innerX + innerY - 2) / 2 && i == Parameters.CircleSplits - 2)
                {
                    _rightBorderElements.Add(_finiteElements.Count - 1);
                }
                else if (j >= (innerX + innerY - 2) / 2 && i == Parameters.CircleSplits - 2)
                {
                    _topBorderElements.Add(_finiteElements.Count - 1);
                }
                
                if (depth <= Parameters.CircleTear.Depth && j == Parameters.CircleTear.Offset - 1)
                {
                    _topTearBorderElements.Add(_finiteElements.Count - 1);
                }
                else if (depth <= Parameters.CircleTear.Depth &&
                         j == Parameters.CircleTear.Offset + Parameters.CircleTear.Split)
                {
                    _rightTearBorderElements.Add(_finiteElements.Count - 1);
                }

                if (depth - Parameters.CircleTear.Depth == 1 && j < (innerX + innerY - 2) / 2 &&
                    j >= Parameters.CircleTear.Offset &&
                    j <= Parameters.CircleTear.Offset + Parameters.CircleTear.Split - 1)
                {
                    _rightTearBorderElements.Add(_finiteElements.Count - 1);
                }
                else if (depth - Parameters.CircleTear.Depth == 1 && j >= (innerX + innerY - 2) / 2 &&
                    j >= Parameters.CircleTear.Offset &&
                    j <= Parameters.CircleTear.Offset + Parameters.CircleTear.Split - 1)
                {
                    _topTearBorderElements.Add(_finiteElements.Count - 1);
                }
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
