using DataStructures.Geometry;

namespace GridBuilder;

public class QuadraticGridBuilder
{
    private GridParameters? Parameters { get; set; }
    
    private Point[] _points = null!;
    private List<FiniteElement> _finiteElements = null!;
    private readonly List<Material> _circleMaterials = new();
    private readonly HashSet<int> _dirichletBoundaries = new();
    private readonly HashSet<Edge3> _neumannBoundaries = new();
    private readonly HashSet<int> _fictitiousPoints = new();
    private readonly HashSet<int> _notFictitiousPoints = new();

    private readonly HashSet<int> _leftBorderElements = new();
    private readonly HashSet<int> _rightBorderElements = new();
    private readonly HashSet<int> _bottomBorderElements = new();
    private readonly HashSet<int> _topBorderElements = new();
    private readonly HashSet<int> _rightTearBorderElements = new();
    private readonly HashSet<int> _topTearBorderElements = new();

    private double[]? _x;
    private double[]? _y;
    
    public (Point[], List<FiniteElement>, HashSet<int>, HashSet<Edge3>, HashSet<int>) BuildGrid(GridParameters parameters)
    {
        Parameters = parameters;
        if (Parameters == null)
            throw new ArgumentNullException(nameof(parameters), "Grid parameters cannot be null.");
        
        CreatePoints();
        GenerateCircleMaterials();
        CreateElements();
        AccountBoundaryConditions();
        
        return (_points, _finiteElements, _dirichletBoundaries, _neumannBoundaries, _fictitiousPoints);
    }

    private void CreatePoints()
    {
        // int innerX = 2 * Parameters!.XInnerSplits + 1;
        // int innerY = 2 * Parameters.YInnerSplits + 1;
        
        int innerX = 1;
        int innerY = 1;
        
        foreach (var segment in Parameters!.CircleMaterials)
        {
            if (segment.Degrees <= 45.0)
            {
                innerY += 2 * segment.Splits;
            }
            else
            {
                innerX += 2 * segment.Splits;
            }
        }

        int circleSplits = innerX + innerY - 1;
        
        _x = new double[innerX + 2 * Parameters.CircleRadiusSplits];
        _y = new double[innerY + 2 * Parameters.CircleRadiusSplits];
        
        _points = new Point[innerX * innerY + 2 * circleSplits * Parameters.CircleRadiusSplits];

        int ky = 0;
        int kx = innerX - 1;
        
        foreach (var segment in Parameters!.CircleMaterials)
        {
            var angleStart = segment.Start * Math.PI / 180.0;
            var angleEnd = segment.Degrees * Math.PI / 180.0;
            var hAngle = (angleEnd - angleStart) / segment.Splits;
            var angle = angleStart;
            
            if (segment.Degrees <= 45.0)
            {

                if (segment.Start == 0.0)
                {
                    _y[ky++] = Parameters.XInterval.RightBorder * Math.Tan(angle);
                }
                
                for (int i = 0; i < segment.Splits; i++)
                {
                    angle += hAngle;
                    _y[ky + 1] = Parameters.XInterval.RightBorder * Math.Tan(angle);
                    _y[ky] = (_y[ky + 1] + _y[ky - 1]) / 2.0;
                    ky += 2;
                }
            }
            else
            {
                if (Math.Abs(segment.Start - 45.0) < 1e-15)
                {
                    _x[kx--] = Parameters.YInterval.RightBorder / Math.Tan(angle);
                }
                
                for (int i = 0; i < segment.Splits; i++)
                {
                    angle += hAngle;
                    _x[kx - 1] = Parameters.YInterval.RightBorder / Math.Tan(angle);
                    _x[kx] = (_x[kx + 1] + _x[kx - 1]) / 2.0;
                    kx -= 2;
                }
            }
        }
        
        double xCirclePoint = Math.Sqrt(Parameters.XInterval.RightBorder * Parameters.XInterval.RightBorder +
                                        Parameters.YInterval.RightBorder * Parameters.YInterval.RightBorder);
        double hxCircle = Math.Abs(Parameters.CircleCoefficient - 1.0) < 1e-14
            ? Parameters.Radius / Parameters.CircleRadiusSplits
            : Parameters.Radius * (1.0 - Parameters.CircleCoefficient) /
              (1.0 - Math.Pow(Parameters.CircleCoefficient, Parameters.CircleRadiusSplits));

        if (Parameters.XInterval.RightBorder + hxCircle / 2.0 > xCirclePoint)
        {
            xCirclePoint = Parameters.XInterval.RightBorder + hxCircle / 2.0;
        }
        else
        {
            hxCircle = Math.Abs(Parameters.CircleCoefficient- 1.0) < 1e-14
                ? (Parameters.Radius - xCirclePoint) / Parameters.CircleRadiusSplits
                : (Parameters.Radius - xCirclePoint) * (1.0 - Parameters.CircleCoefficient) /
                  (1.0 - Math.Pow(Parameters.CircleCoefficient, Parameters.CircleRadiusSplits));
        }
        
        for (int i = 0; i < 2 * Parameters.CircleRadiusSplits; i+=2)
        {
            _x[innerX + i] = xCirclePoint;
            xCirclePoint += hxCircle / 2.0;
            _x[innerX + i + 1] = xCirclePoint;
            xCirclePoint += hxCircle / 2.0;
            hxCircle *= Parameters.CircleCoefficient;
        }
        
        double yCirclePoint = Math.Sqrt(Parameters.YInterval.RightBorder * Parameters.YInterval.RightBorder +
                                        Parameters.XInterval.RightBorder * Parameters.XInterval.RightBorder);
        double hyCircle = Math.Abs(Parameters.CircleCoefficient- 1.0) < 1e-14
            ? Parameters.Radius / Parameters.CircleRadiusSplits
            : Parameters.Radius * (1.0 - Parameters.CircleCoefficient) /
              (1.0 - Math.Pow(Parameters.CircleCoefficient, Parameters.CircleRadiusSplits));

        if (Parameters.YInterval.RightBorder + hyCircle / 2.0 > yCirclePoint)
        {
            yCirclePoint = Parameters.XInterval.RightBorder + hyCircle / 2.0;
        }
        else
        {
            hyCircle = Math.Abs(Parameters.CircleCoefficient- 1.0) < 1e-14
                ? (Parameters.Radius - yCirclePoint) / Parameters.CircleRadiusSplits
                : (Parameters.Radius - yCirclePoint) * (1.0 - Parameters.CircleCoefficient) /
                  (1.0 - Math.Pow(Parameters.CircleCoefficient, Parameters.CircleRadiusSplits));
        }
        
        for (int i = 0; i < 2 * Parameters.CircleRadiusSplits; i+=2)
        {
            _y[innerY + i] = yCirclePoint;
            yCirclePoint += hyCircle / 2.0;
            _y[innerY + i + 1] = yCirclePoint;
            yCirclePoint += hyCircle / 2.0;
            hyCircle *= Parameters.CircleCoefficient;
        }

        int iPoint = 0;
        // Inner scope
        for (int i = 0; i < innerY; i++)
        {
            for (int j = 0; j < innerX; j++)
            {
                _points[iPoint++] = new(_x[j], _y[i]);
            }
        }
        
        // Transition from inner to circle scope
        List<Point> points = new List<Point>();
        
        Point[] previousPoints = new Point[Parameters.XInnerSplits + Parameters.YInnerSplits + 1];
        for (int i = 0; i < Parameters.YInnerSplits; i++)
        {
            previousPoints[i] = new Point(_x[innerX - 1], _y[2 * i]);
        }
        
        previousPoints[Parameters.YInnerSplits] = new Point(_x[innerX - 1], _y[innerY - 1]);

        for (int i = Parameters.XInnerSplits; i > 0; i--)
        {
            previousPoints[Parameters.XInnerSplits - i + Parameters.YInnerSplits + 1] = new Point(_x[2 * i - 2], _y[innerY - 1]);
        }

        for (int i = 0; i < Parameters.CircleRadiusSplits; i++)
        {
            double radius = _x[innerX + 1 + 2 * i];
            int kek = 0;
            bool flag = true;
            
            foreach (var segment in Parameters!.CircleMaterials)
            {
                var angleStart = segment.Start * Math.PI / 180.0;
                var angleEnd = segment.Degrees * Math.PI / 180.0;
                var hAngle = (angleEnd - angleStart) / segment.Splits;
                var angle = angleStart;
                
                for (int j = 0; j < segment.Splits; j++)
                {
                    Point mainPoint = new(radius * Math.Cos(angle), radius * Math.Sin(angle));
                    
                    if (flag)
                    {
                        points.Add(new(mainPoint.X, mainPoint.Y));
                        _points[iPoint++] = new Point((points[^1].X + previousPoints[kek].X) / 2.0,
                            (points[^1].Y + previousPoints[kek].Y) / 2.0);
                        previousPoints[kek] = new Point(points[^1].X, points[^1].Y);
                        kek++;
                        flag = false;
                    }
                    else
                    {
                        points.Add(new Point((mainPoint.X + points[^1].X) / 2.0, (mainPoint.Y + points[^1].Y) / 2.0));
                        points.Add(new(mainPoint.X, mainPoint.Y));
                        _points[++iPoint] = new Point((points[^1].X + previousPoints[kek].X) / 2.0,
                            (points[^1].Y + previousPoints[kek].Y) / 2.0);
                        _points[iPoint - 1] = new Point((_points[iPoint].X + _points[iPoint - 2].X) / 2.0,
                            (_points[iPoint].Y + _points[iPoint - 2].Y) / 2.0);
                        previousPoints[kek] = new Point(points[^1].X, points[^1].Y);
                        kek++;
                        iPoint++;
                    }

                    angle += hAngle;
                }
            }

            Point p = new Point(radius * Math.Cos(Parameters.CircleMaterials[^1].Degrees * Math.PI / 180.0),
                radius * Math.Sin(Parameters.CircleMaterials[^1].Degrees * Math.PI / 180.0));
            
            points.Add(new Point((p.X + points[^1].X) / 2.0, (p.Y + points[^1].Y) / 2.0));
            points.Add(new Point(p.X, p.Y));
            _points[++iPoint] = new Point((points[^1].X + previousPoints[kek].X) / 2.0,
                (points[^1].Y + previousPoints[kek].Y) / 2.0);
            _points[iPoint - 1] = new Point((_points[iPoint].X + _points[iPoint - 2].X) / 2.0,
                (_points[iPoint].Y + _points[iPoint - 2].Y) / 2.0);
            previousPoints[kek] = new Point(points[^1].X, points[^1].Y);
            iPoint++;
            kek++;
            
            foreach (var point in points)
            {
                _points[iPoint++] = point;
            }
            
            points.Clear();
        }
    }

    private void GenerateCircleMaterials()
    {
        foreach (var i in Parameters!.CircleMaterials)
        {
            for (int j = 0; j < i.Splits; j++)
            {
                _circleMaterials.Add(i.Material);
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
        int circleXSkip = 2 * Parameters.YInnerSplits;

        // Inner scope
        for (int i = 0; i < Parameters.YInnerSplits; i++)
        {
            for (int j = 0; j < Parameters.XInnerSplits; j++)
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
        for (int i = 0; i < Parameters.YInnerSplits; i++)
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
        for (int i = Parameters.XInnerSplits; i > 0; i--)
        {
            nodes[0] = innerSkip - (innerX - 2 * i) - 2;
            nodes[1] = innerSkip - (innerX - 2 * i) - 1;
            nodes[2] = innerSkip - (innerX - 2 * i);

            nodes[3] = innerSkip + circleXSkip + 2 + (innerX - 1 - 2 * i);
            nodes[4] = innerSkip + circleXSkip + 1 + (innerX - 1 - 2 * i);
            nodes[5] = innerSkip + circleXSkip + (innerX - 1 - 2 * i);

            nodes[6] = innerSkip + circleRow + circleXSkip + 2 + (innerX - 1 - 2 * i);
            nodes[7] = innerSkip + circleRow + circleXSkip + 1 + (innerX - 1 - 2 * i);
            nodes[8] = innerSkip + circleRow + circleXSkip + (innerX - 1 - 2 * i);
            
            _finiteElements.Add(new FiniteElement(nodes.ToArray(), _circleMaterials[materialCounter++], FiniteElementType.Quadratic));
        }
        _leftBorderElements.Add(_finiteElements.Count - 1);

        // Circle scope
        int depth = Parameters.CircleRadiusSplits - 1;
        for (int i = 0; i < Parameters.CircleRadiusSplits - 1; i++)
        {
            materialCounter = 0;
            for (int j = 0; j < (innerX + innerY - 2) / 2; j++)
            {
                if (depth <= Parameters.CircleTear.Depth && depth >= Parameters.CircleTear.Height)
                {
                    if (j >= Parameters.CircleTear.Offset &&
                        j <= Parameters.CircleTear.Offset + Parameters.CircleTear.Split - 1)
                    {
                        if (j < (innerX + innerY - 2) / 4)
                        {
                            int firstPoint = innerSkip + circleRow + 2 * i * circleRow + 2 * j;
                        
                            _fictitiousPoints.Add(firstPoint);
                            _fictitiousPoints.Add(firstPoint + circleRow);
                            _fictitiousPoints.Add(firstPoint + 2 * circleRow);
                            
                            _fictitiousPoints.Add(firstPoint + 1);
                            _fictitiousPoints.Add(firstPoint + circleRow + 1);
                            _fictitiousPoints.Add(firstPoint + 2 * circleRow + 1);
                            
                            _fictitiousPoints.Add(firstPoint + 2);
                            _fictitiousPoints.Add(firstPoint + circleRow + 2);
                            _fictitiousPoints.Add(firstPoint + 2 * circleRow + 2);
                        }
                        else
                        {
                            int firstPoint = innerSkip + circleRow + 2 * i * circleRow + 2 * j + 2;
                            
                            _fictitiousPoints.Add(firstPoint);
                            _fictitiousPoints.Add(firstPoint - 1);
                            _fictitiousPoints.Add(firstPoint - 2);
                            
                            _fictitiousPoints.Add(firstPoint + circleRow);
                            _fictitiousPoints.Add(firstPoint + circleRow - 1);
                            _fictitiousPoints.Add(firstPoint + circleRow - 2);
                            
                            _fictitiousPoints.Add(firstPoint + 2 * circleRow);
                            _fictitiousPoints.Add(firstPoint + 2 * circleRow - 1);
                            _fictitiousPoints.Add(firstPoint + 2 * circleRow - 2);
                        }
                        
                        materialCounter++;
                        continue;
                    }
                }
                
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
                    
                    _notFictitiousPoints.Add(firstPoint);
                    _notFictitiousPoints.Add(firstPoint + circleRow);
                    _notFictitiousPoints.Add(firstPoint + 2 * circleRow);
                    
                    _notFictitiousPoints.Add(firstPoint + 1);
                    _notFictitiousPoints.Add(firstPoint + circleRow + 1);
                    _notFictitiousPoints.Add(firstPoint + 2 * circleRow + 1);
                    
                    _notFictitiousPoints.Add(firstPoint + 2);
                    _notFictitiousPoints.Add(firstPoint + circleRow + 2);
                    _notFictitiousPoints.Add(firstPoint + 2 * circleRow + 2);
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
                    
                    _notFictitiousPoints.Add(firstPoint);
                    _notFictitiousPoints.Add(firstPoint - 1);
                    _notFictitiousPoints.Add(firstPoint - 2);
                    
                    _notFictitiousPoints.Add(firstPoint + circleRow);
                    _notFictitiousPoints.Add(firstPoint + circleRow - 1);
                    _notFictitiousPoints.Add(firstPoint + circleRow - 2);
                    
                    _notFictitiousPoints.Add(firstPoint + 2 * circleRow);
                    _notFictitiousPoints.Add(firstPoint + 2 * circleRow - 1);
                    _notFictitiousPoints.Add(firstPoint + 2 * circleRow - 2);
                }
                
                _finiteElements.Add(new FiniteElement(nodes.ToArray(), _circleMaterials[materialCounter++], FiniteElementType.Quadratic));

                if (j == 0)
                {
                    _bottomBorderElements.Add(_finiteElements.Count - 1);
                }

                if (j == (innerX + innerY) / 2 - 2)
                {
                    _leftBorderElements.Add(_finiteElements.Count - 1);
                }

                if (j < (innerX + innerY - 2) / 4 && i == Parameters.CircleRadiusSplits - 2)
                {
                    _rightBorderElements.Add(_finiteElements.Count - 1);
                }
                else if (j >= (innerX + innerY - 2) / 4 && i == Parameters.CircleRadiusSplits - 2)
                {
                    _topBorderElements.Add(_finiteElements.Count - 1);
                }
                
                if (Parameters.CircleTear.Height == 1)
                {
                    if (depth <= Parameters.CircleTear.Depth && j == Parameters.CircleTear.Offset - 1)
                    {
                        _topTearBorderElements.Add(_finiteElements.Count - 1);
                    }
                    else if (depth <= Parameters.CircleTear.Depth &&
                             j == Parameters.CircleTear.Offset + Parameters.CircleTear.Split)
                    {
                        _rightTearBorderElements.Add(_finiteElements.Count - 1);
                    }

                    if (depth - Parameters.CircleTear.Depth == 1 && j < (innerX + innerY - 2) / 4 &&
                        j >= Parameters.CircleTear.Offset &&
                        j <= Parameters.CircleTear.Offset + Parameters.CircleTear.Split - 1)
                    {
                        _rightTearBorderElements.Add(_finiteElements.Count - 1);
                    }
                    else if (depth - Parameters.CircleTear.Depth == 1 && j >= (innerX + innerY - 2) / 4 &&
                        j >= Parameters.CircleTear.Offset &&
                        j <= Parameters.CircleTear.Offset + Parameters.CircleTear.Split - 1)
                    {
                        _topTearBorderElements.Add(_finiteElements.Count - 1);
                    }
                }
            }

            depth--;
        }

        foreach (var fictitious in _fictitiousPoints)
        {
            if (_notFictitiousPoints.Contains(fictitious))
            {
                _fictitiousPoints.Remove(fictitious);
            }
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
                    _dirichletBoundaries.Add(_finiteElements[element].Nodes[3]);
                    _dirichletBoundaries.Add(_finiteElements[element].Nodes[6]);
                    break;
                case 2:
                    _neumannBoundaries.Add(new Edge3(_finiteElements[element].Nodes[0],
                        _finiteElements[element].Nodes[3], _finiteElements[element].Nodes[6],
                        _finiteElements[element].ElementMaterial));
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
                    _dirichletBoundaries.Add(_finiteElements[element].Nodes[2]);
                    break;
                case 2:
                    _neumannBoundaries.Add(new Edge3(_finiteElements[element].Nodes[0],
                        _finiteElements[element].Nodes[1], _finiteElements[element].Nodes[2],
                        _finiteElements[element].ElementMaterial));
                    break;
            }
        }
        
        foreach (var element in _topBorderElements)
        {
            switch (Parameters!.CircleBorder)
            {
                case 1:
                    _dirichletBoundaries.Add(_finiteElements[element].Nodes[6]);
                    _dirichletBoundaries.Add(_finiteElements[element].Nodes[7]);
                    _dirichletBoundaries.Add(_finiteElements[element].Nodes[8]);
                    break;
                case 2:
                    _neumannBoundaries.Add(new Edge3(_finiteElements[element].Nodes[6],
                        _finiteElements[element].Nodes[7], _finiteElements[element].Nodes[8],
                        _finiteElements[element].ElementMaterial));
                    break;
            }
        }
        
        foreach (var element in _rightBorderElements)
        {
            switch (Parameters!.CircleBorder)
            {
                case 1:
                    _dirichletBoundaries.Add(_finiteElements[element].Nodes[2]);
                    _dirichletBoundaries.Add(_finiteElements[element].Nodes[5]);
                    _dirichletBoundaries.Add(_finiteElements[element].Nodes[8]);
                    break;
                case 2:
                    _neumannBoundaries.Add(new Edge3(_finiteElements[element].Nodes[2],
                        _finiteElements[element].Nodes[5], _finiteElements[element].Nodes[8],
                        _finiteElements[element].ElementMaterial));
                    break;
            }
        }

        foreach (var element in _topTearBorderElements)
        {
            switch (Parameters!.CircleTearBorder)
            {
                case 1:
                    _dirichletBoundaries.Add(_finiteElements[element].Nodes[6]);
                    _dirichletBoundaries.Add(_finiteElements[element].Nodes[7]);
                    _dirichletBoundaries.Add(_finiteElements[element].Nodes[8]);
                    break;
                case 2:
                    _neumannBoundaries.Add(new Edge3(_finiteElements[element].Nodes[6],
                        _finiteElements[element].Nodes[7], _finiteElements[element].Nodes[8],
                        _finiteElements[element].ElementMaterial));
                    break;
            }
        }
        
        foreach (var element in _rightTearBorderElements)
        {
            switch (Parameters!.CircleTearBorder)
            {
                case 1:
                    _dirichletBoundaries.Add(_finiteElements[element].Nodes[2]);
                    _dirichletBoundaries.Add(_finiteElements[element].Nodes[5]);
                    _dirichletBoundaries.Add(_finiteElements[element].Nodes[8]);
                    break;
                case 2:
                    _neumannBoundaries.Add(new Edge3(_finiteElements[element].Nodes[2],
                        _finiteElements[element].Nodes[5], _finiteElements[element].Nodes[8],
                        _finiteElements[element].ElementMaterial));
                    break;
            }
        }
    }
}