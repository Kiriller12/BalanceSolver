using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Accord.Math;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra.Double;
using MathWorks.MATLAB.NET.Arrays;
using QPSolver;

namespace Balance
{
    public class MathlabBalanceSolver : IBalanceSolver
    {
        public double DisbalanceOriginal { get; private set; }
        public double Disbalance { get; private set; }
        public TimeSpan Time { get; private set; }
        public TimeSpan TimeAll { get; private set; }

        public double[] Solve(double[] x0, double[,] a, double[] b, double[] measurability, double[] tolerance,
            double[] lower, double[] upper)
        {
            // Проверка аргументов на null
            _ = x0 ?? throw new ArgumentNullException(nameof(x0));
            _ = a ?? throw new ArgumentNullException(nameof(a));
            _ = b ?? throw new ArgumentNullException(nameof(b));
            _ = measurability ?? throw new ArgumentNullException(nameof(measurability));
            _ = tolerance ?? throw new ArgumentNullException(nameof(tolerance));
            _ = lower ?? throw new ArgumentNullException(nameof(lower));
            _ = upper ?? throw new ArgumentNullException(nameof(upper));

            //Проверка аргументов на размерности
            if(x0.Length == 0) throw new ArgumentException(nameof(x0));
            if (a.GetLength(1) != x0.Length)
                throw new ArgumentException("Array length by dimension 1 is not equal to X0 length.", nameof(a));
            if (b.Length != a.GetLength(0))
                throw new ArgumentException("Array length is not equal to A length by 0 dimension.", nameof(b));
            if (measurability.Length != x0.Length)
                throw new ArgumentException("Array length is not equal to X0 length.", nameof(measurability));
            if (tolerance.Length != x0.Length)
                throw new ArgumentException("Array length is not equal to X0 length.", nameof(tolerance));
            if (lower.Length != x0.Length)
                throw new ArgumentException("Array length is not equal to X0 length.", nameof(lower));
            if (upper.Length != x0.Length)
                throw new ArgumentException("Array length is not equal to X0 length.", nameof(upper));

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            try
            {
                //Создаем объет для связи с матлабом
                var matlab = new MatlabWorker();

                // Преобразуем данные в понятный матлабу формат
                var aM = new MWNumericArray(a);
                var bM = new MWNumericArray(b.Length, 1, b);
                var x0M = new MWNumericArray(x0.Length, 1, x0);
                var toleranceM = new MWNumericArray(tolerance.Length, 1, tolerance);
                var measurabilityM = new MWNumericArray(measurability.Length, 1, measurability);
                var lowerM = new MWNumericArray(lower.Length, 1, lower);
                var upperM = new MWNumericArray(upper.Length, 1, upper);
                var maxIter = new MWNumericArray(200);
                var drTol = new MWNumericArray(0.0);

                // Запускаем солвер и ищем решение
                var result = matlab.QPSolver(7, aM, bM, x0M, toleranceM, measurabilityM,
                    lowerM, upperM, maxIter, drTol);

                stopWatch.Stop();
                Time = TimeSpan.Zero;
                TimeAll = stopWatch.Elapsed;

                DisbalanceOriginal = ((MWNumericArray)result[2]).ToScalarDouble();
                Disbalance = ((MWNumericArray)result[3]).ToScalarDouble();

                return (double[])((MWNumericArray)result[0]).ToVector(MWArrayComponent.Real);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                throw new ApplicationException("Failed to solve balance task.");
            }
        }

        public double GlobalTest(double[] x0, double[,] a, double[] measurability, double[] tolerance)
        {
            var aMatrix = SparseMatrix.OfArray(a);
            var aTransposedMatrix = SparseMatrix.OfMatrix(aMatrix.Transpose());
            var x0Vector = SparseVector.OfEnumerable(x0);

            // Введение погрешностей по неизмеряемым потокам
            var xStd = SparseVector.OfEnumerable(tolerance) / 1.96;

            for (var i = 0; i < xStd.Count; i++)
            {
                if(Math.Abs(measurability[i]) < 0.0000001)
                {
                    xStd[i] = Math.Pow(10, 2) * x0Vector.Maximum();
                }
            }

            var sigma = SparseMatrix.OfDiagonalVector(xStd.PointwisePower(2));

            var r = aMatrix * x0Vector;
            var v = aMatrix * sigma * aTransposedMatrix;

            var result =  r * v.PseudoInverse() * r.ToColumnMatrix();
            var chi = ChiSquared.InvCDF(aMatrix.RowCount, 1 - 0.05);

            return result[0] / chi;
        }

        public double[,] GlrTest(double[] x0, double[,] a, double[] measurability, double[] tolerance, 
            IEnumerable<(int, int, int)> flows, double globalTest)
        {
            var nodesCount = a.GetLength(0);

            var glrTable = new double[nodesCount, nodesCount];

            if (flows != null)
            {
                foreach (var flow in flows)
                {
                    var (i, j, _) = flow;

                    // Добавляем новый поток
                    var aColumn = new double[nodesCount];
                    aColumn[i] = 1;
                    aColumn[j] = -1;

                    var aNew = a.InsertColumn(aColumn);
                    var x0New = x0.Append(0).ToArray();
                    var measurabilityNew = measurability.Append(0).ToArray();
                    var toleranceNew = tolerance.Append(0).ToArray();

                    // Считаем тест и находим разницу
                    glrTable[i, j] = globalTest - GlobalTest(x0New, aNew, measurabilityNew, toleranceNew);
                }
            }
            else
            {
                for (var i = 0; i < nodesCount; i++)
                {
                    for (var j = i + 1; j < nodesCount; j++)
                    {
                        // Добавляем новый поток
                        var aColumn = new double[nodesCount];
                        aColumn[i] = 1;
                        aColumn[j] = -1;

                        var aNew = a.InsertColumn(aColumn);
                        var x0New = x0.Append(0).ToArray();
                        var measurabilityNew = measurability.Append(0).ToArray();
                        var toleranceNew = tolerance.Append(0).ToArray();

                        // Считаем тест и находим разницу
                        glrTable[i, j] = globalTest - GlobalTest(x0New, aNew, measurabilityNew, toleranceNew);
                    }
                }
            }

            return glrTable;
        }

        public IEnumerable<(int, int, int)> GetFlows(double[,] a)
        {
            var flows = new List<(int, int, int)>();
            for (var k = 0; k < a.GetLength(1); k++)
            {
                var column = a.GetColumn(k);

                var i = column.IndexOf(-1);
                var j = column.IndexOf(1);

                if (i == -1 || j == -1)
                {
                    continue;
                }

                flows.Add((i, j, k));
            }

            return flows;
        }
    }
}
