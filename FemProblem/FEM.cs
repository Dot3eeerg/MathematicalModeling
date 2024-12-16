using DataStructures;
using DataStructures.Geometry;
using GridBuilder;

namespace FemProblem;

public class FEM
{
    private Grid _grid = default!;
    private ITest _test = default!;
    private IBasis2D _basis = default!;
    private IterativeSolver _iterativeSolver = default!;
    private Vector<double> _localVector = default!;
    private Vector<double> _globalVector = default!;
    private SLAEAssembler _slaeAssembler = default!;
    
    public void SetGrid(Grid grid) => _grid = grid;
    
    public void SetTest(ITest test) => _test = test;
    
    public void SetBasis(IBasis2D basis) => _basis = basis;

    public void Solve()
    {
        Initialize();
        AssemblySlae();
        AccountNeumannBoundaries();
        AccountDirichletBoundaries();
        AccountFictitiousNodes();
        
        _iterativeSolver.SetMatrix(_slaeAssembler.GlobalMatrix);
        _iterativeSolver.SetVector(_globalVector);
        _iterativeSolver.Compute();
        
        SaveSolution("Grid");
        CalculateError();
    }

    private void CalculateError()
    {
        var error = new double[_grid.Nodes.Count];
        
        for (int i = 0; i < error.Length; i++)
        {
            if (_grid.FictitiousNodes != null && _grid.FictitiousNodes.Contains(i)) continue;
            error[i] = Math.Abs(_iterativeSolver.Solution!.Value[i] - _test.U(_grid.Nodes[i]));
        }
        
        Array.ForEach(error, Console.WriteLine);
        
        var sum = error.Sum(t => t * t);
        
        sum = Math.Sqrt(sum / _grid.Nodes.Count);
        
        Console.WriteLine($"rms = {sum}");
        
        // using var sw = new StreamWriter("1.csv");
        //
        // for (int i = 0; i < error.Length; i++)
        // {
        //     if (i == 0)
        //     {
        //         Console.WriteLine($"{i}, {_test.U(_grid.Nodes[i])}, {_iterativeSolver.Solution!.Value[i]}, {error[i]}, {sum}");
        //         // sw.WriteLine("$i$, $u_i^*$, $u_i$, $|u^* - u|$, Погрешность");
        //         sw.WriteLine(
        //             $"{i}, {_test.U(_grid.Nodes[i])}, {_iterativeSolver.Solution!.Value[i]}, {error[i]}, {sum}");
        //         continue;
        //     }
        //
        //     if (_grid.FictitiousNodes != null && _grid.FictitiousNodes.Contains(i))
        //     {
        //         Console.WriteLine($"{i} Fictiotious node");
        //     }
        //     else
        //     {
        //         sw.WriteLine($"{i}, {_test.U(_grid.Nodes[i])}, {_iterativeSolver.Solution!.Value[i]}, {error[i]},");
        //         Console.WriteLine($"{i}, {_test.U(_grid.Nodes[i])}, {_iterativeSolver.Solution!.Value[i]}, {error[i]}, {sum}");
        //     }
        // }
    }

    private void SaveSolution(string folder)
    {
        var sw = new StreamWriter($"{folder}/solution");
        foreach (var value in _iterativeSolver.Solution!)
        {
            sw.WriteLine(value);
        }
        sw.Close();
    }
    
    private void Initialize()
    {
        _slaeAssembler = new SLAEAssembler();
        
        PortraitBuilder.Build(_grid, out var ig, out var jg);
        _slaeAssembler.GlobalMatrix = new(ig.Length - 1, jg.Length)
        {
            Ig = ig,
            Jg = jg
        };
        _slaeAssembler.SetBasis(_basis);
        _slaeAssembler.Grid = _grid;

        _globalVector = new(ig.Length - 1);
        _localVector = new(_slaeAssembler.Basis.Size);

        _iterativeSolver = new CGMCholesky(5000, 1e-16);
    }

    private void AssemblySlae()
    {
        for (int iElem = 0; iElem < _grid.FiniteElements!.Count; iElem++)
        {
            var element = _grid.FiniteElements[iElem];

            _slaeAssembler.BuildLocalMatrices(iElem);
            BuildLocalVector(iElem);

            for (int i = 0; i < _slaeAssembler.Basis.Size; i++)
            {
                _globalVector[element.Nodes[i]] += _localVector[i];

                for (int j = 0; j < _slaeAssembler.Basis.Size; j++)
                {
                    _slaeAssembler.FillGlobalMatrix(element.Nodes[i], element.Nodes[j],
                        _slaeAssembler.StiffnessMatrix[i, j]);
                }
            }
        } 
    }
    
    private void BuildLocalVector(int iElem)
    {
        _localVector.Fill(0.0);

        for (int i = 0; i < _slaeAssembler.Basis.Size; i++)
        {
            for (int j = 0; j < _slaeAssembler.Basis.Size; j++)
            {
                _localVector[i] += _slaeAssembler.MassMatrix[i, j] *
                                   _test.F(_grid.Nodes![_grid.FiniteElements![iElem].Nodes[j]],
                                       _grid.FiniteElements[iElem].ElementMaterial);
            }
        }
    }

    private void AccountDirichletBoundaries()
    {
        foreach (var node in _grid.DirichletNodes!)
        {
            _slaeAssembler.GlobalMatrix.Di[node] = 1;
            var value = _test.U(_grid.Nodes![node]);
            _globalVector[node] = value;

            for (int i = _slaeAssembler.GlobalMatrix.Ig[node]; i < _slaeAssembler.GlobalMatrix.Ig[node + 1]; i++)
            {
                _globalVector[_slaeAssembler.GlobalMatrix.Jg[i]] -= value * _slaeAssembler.GlobalMatrix.Gg[i];
                _slaeAssembler.GlobalMatrix.Gg[i] = 0;
            }

            for (int i = node + 1; i < _slaeAssembler.GlobalMatrix.Size; i++)
            {
                for (int j = _slaeAssembler.GlobalMatrix.Ig[i]; j < _slaeAssembler.GlobalMatrix.Ig[i + 1]; j++)
                {
                    if (_slaeAssembler.GlobalMatrix.Jg[j] == node)
                    {
                        _globalVector[i] -= value * _slaeAssembler.GlobalMatrix.Gg[j];
                        _slaeAssembler.GlobalMatrix.Gg[j] = 0;
                    }
                }
            }
        }
    }

    private void AccountNeumannBoundaries()
    {
        if (_grid.NeumannEdges != null)
        {
            var localMassMatrix = new Matrix(3)
            {
                [0, 0] = 4.0 / 30.0,
                [0, 1] = 2.0 / 30.0,
                [0, 2] = -1.0 / 30.0,
                [1, 0] = 2.0 / 30.0,
                [1, 1] = 16.0 / 30.0,
                [1, 2] = 2.0 / 30.0,
                [2, 0] = -1.0 / 30.0,
                [2, 1] = 2.0 / 30.0,
                [2, 2] = 4.0 / 30.0,
            };
            var thetaFunction = new Vector<double>(3);
            
            foreach (var edge in _grid.NeumannEdges)
            {
                double length = Math.Sqrt(
                    (_grid.Nodes![edge.Node3].X - _grid.Nodes[edge.Node1].X) *
                    (_grid.Nodes[edge.Node3].X - _grid.Nodes[edge.Node1].X) +
                    (_grid.Nodes[edge.Node3].Y - _grid.Nodes[edge.Node1].Y) *
                    (_grid.Nodes[edge.Node3].Y - _grid.Nodes[edge.Node1].Y));
                
                thetaFunction[0] = _test.Theta(_grid.Nodes[edge.Node1], edge.Material);
                thetaFunction[1] = _test.Theta(_grid.Nodes[edge.Node2], edge.Material);
                thetaFunction[2] = _test.Theta(_grid.Nodes[edge.Node3],edge.Material);
    
                for (int i = 0; i < localMassMatrix.Size; i++)
                {
                    for (int j = 0; j < localMassMatrix.Size; j++)
                    {
                        _localVector[i] += localMassMatrix[i, j] * thetaFunction[j];
                    }
                }
    
                _globalVector[edge.Node1] += length * _localVector[0];
                _globalVector[edge.Node2] += length * _localVector[1];
                _globalVector[edge.Node3] += length * _localVector[2];
                
                _localVector.Fill(0.0);
            }
        }
    }

    private void AccountFictitiousNodes()
    {
        foreach (var node in _grid.FictitiousNodes!)
        {
            _slaeAssembler.GlobalMatrix.Di[node] = 1;
            _globalVector[node] = 0;
        }
    }
}