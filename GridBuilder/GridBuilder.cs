using DataStructures.Geometry;

namespace GridBuilder;

public class GridBuilder
{
    private GridParameters? Parameters { get; set; }

    private Point[] _points = null!;
    private List<FiniteElement> _finiteElements = null!;
    
    public (Point[], List<FiniteElement>) BuildGrid(GridParameters parameters)
    {
        Parameters = parameters;
        if (Parameters == null)
            throw new ArgumentNullException(nameof(parameters), "Grid parameters cannot be null.");
        
        CreatePoints();
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
        for (int i = 0; i < Parameters.CircleSplits; i++)
        {
            double radius = x[innerX + i];
            double theta = Math.PI / 2  / (innerX + innerY - 2);

            for (int j = 0; j < innerX + innerY - 1; j++)
            {
                var angle = j * theta;
                _points[iPoint++] = new(radius * Math.Cos(angle), radius * Math.Sin(angle));
            }
        }
    }
    
    private void CreateElements()
    {
        
    }

    private void AccountBoundaryConditions()
    {
        
    }
}