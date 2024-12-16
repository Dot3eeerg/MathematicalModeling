using System.Numerics;
using DataStructures;
using DataStructures.Geometry;
using GridBuilder;
using MathFem;

namespace FemProblem;

public class SLAEAssembler
{ 
   private Grid _grid = default!;
   public Grid Grid
   {
       get => _grid;
       set => _grid = value ?? throw new ArgumentNullException(nameof(value));
   }

   private Integration _integrator = new Integration(new SegmentGaussOrder5());
   public IBasis2D Basis;
   
   public SparseMatrix GlobalMatrix { get; set; } = default!;
   public Matrix StiffnessMatrix { get; private set; } = default!;
   public Matrix MassMatrix { get; private set; } = default!;

   public void SetBasis(IBasis2D basis)
   {
       Basis = basis ?? throw new ArgumentNullException(nameof(basis));
       StiffnessMatrix = new Matrix(Basis.Size);
       MassMatrix = new Matrix(Basis.Size);
   }
   
   public void FillGlobalMatrix(int i, int j, double value)
   {
       if (GlobalMatrix is null)
       {
           throw new("Initialize the global matrix (use portrait builder)!");
       }

       if (i == j)
       {
           GlobalMatrix.Di[i] += value;
           return;
       }

       if (i <= j) return;
       for (int ind = GlobalMatrix.Ig[i]; ind < GlobalMatrix.Ig[i + 1]; ind++)
       {
           if (GlobalMatrix.Jg[ind] != j) continue;
           GlobalMatrix.Gg[ind] += value;
           return;
       }
   } 
   
   public void BuildLocalMatrices(int iElem)
    {
        var templateElement = new Rectangle(new(0.0, 0.0), new(1.0, 1.0));

        for (int i = 0; i < Basis.Size; i++)
        {
            var i1 = i;
            
            for (int j = 0; j <= i; j++)
            {
                var j1 = j;
                var function = double(Point p) =>
                {
                    var dxFi1 = Basis.GetDPsi(i1, 0, p);
                    var dxFi2 = Basis.GetDPsi(j1, 0, p);
                    var dyFi1 = Basis.GetDPsi(i1, 1, p);
                    var dyFi2 = Basis.GetDPsi(j1, 1, p);

                    var calculates = CalculateJacobian(iElem, p);
                    var vector1 = new DataStructures.Vector<double>(calculates.Reverse.Size) { new[] { dxFi1, dyFi1 } };
                    var vector2 = new DataStructures.Vector<double>(calculates.Reverse.Size) { new[] { dxFi2, dyFi2 } };

                    return calculates.Reverse * vector1 * (calculates.Reverse * vector2) *
                           Math.Abs(calculates.Determinant);
                };

                StiffnessMatrix[i, j] =
                    StiffnessMatrix[j, i] = _integrator.Gauss2D(function, templateElement);

                function = p =>
                {
                    var fi1 = Basis.GetPsi(i1, p);
                    var fi2 = Basis.GetPsi(j1, p);
                    var calculates = CalculateJacobian(iElem, p);

                    return fi1 * fi2 * Math.Abs(calculates.Determinant);
                };
                MassMatrix![i, j] = MassMatrix[j, i] =
                    _integrator.Gauss2D(function, templateElement);
            }
        }
        
        for (int i = 0; i < Basis.Size; i++)
        {
            for (int j = 0; j <= i; j++)
            {
                StiffnessMatrix[i, j] =
                    StiffnessMatrix[j, i] =
                        _grid.FiniteElements![iElem].ElementMaterial.Lambda * StiffnessMatrix[i, j] +
                        _grid.FiniteElements[iElem].ElementMaterial.Gamma * MassMatrix[i, j];
                // _grid.FiniteElements![iElem].Material * StiffnessMatrix[i, j];
            }
        }
    }

    private (double Determinant, Matrix Reverse) CalculateJacobian(int iElem, Point point)
    {
        Span<double> dx = stackalloc double[2];
        Span<double> dy = stackalloc double[2];

        var element = _grid.FiniteElements![iElem];
        var basis = Basis;

        for (int i = 0; i < basis.Size; i++)
        {
            for (int k = 0; k < 2; k++)
            {
                dx[k] += basis.GetDPsi(i, k, point) * _grid.Nodes![element.Nodes[i]].X;
                dy[k] += basis.GetDPsi(i, k, point) * _grid.Nodes[element.Nodes[i]].Y;
            }
        }

        var determinant = dx[0] * dy[1] - dx[1] * dy[0];

        var reverse = new Matrix(2)
        {
            [0, 0] = dy[1],
            [0, 1] = -dy[0],
            [1, 0] = -dx[1],
            [1, 1] = dx[0]
        };

        return (determinant, 1.0 / determinant * reverse);
    }
}