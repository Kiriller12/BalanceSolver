using System;
using System.Collections.Generic;
using Accord.Math;
using Accord.Math.Optimization;

namespace Balance
{
    public class AccordBalanceSolver : IBalanceSolver
    {
        public double DisbalanceOriginal { get; private set; }
        public double Disbalance { get; private set; }

        public double[] Solve(double[] x0, double[,] a, double[] b, double[] measurability, double[] tolerance, double[] lower, double[] upper)
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

            var i = Matrix.Diagonal(measurability);
            var w = Matrix.Diagonal(1.Divide(tolerance.Pow(2)));

            var h = i.Dot(w);
            var d = h.Dot(x0).Multiply(-1);

            var func = new QuadraticObjectiveFunction(h, d);
            var constraints = new List<LinearConstraint>();

            //Нижние и верхние границы
            for (var j = 0; j < x0.Length; j++)
            {
                constraints.Add(new LinearConstraint(1)
                {
                    VariablesAtIndices = new[] { j },
                    ShouldBe = ConstraintType.GreaterThanOrEqualTo,
                    Value = lower[j]
                });

                constraints.Add(new LinearConstraint(1)
                {
                    VariablesAtIndices = new[] { j },
                    ShouldBe = ConstraintType.LesserThanOrEqualTo,
                    Value = upper[j]
                });
            }

            //Ограничения для решения задачи баланса
            for (var j = 0; j < b.Length; j++)
            {
                var notNullElements = Array.FindAll(a.GetRow(j), x => Math.Abs(x) > 0.0000001);
                var notNullElementsIndexes = new List<int>();
                for (var k = 0; k < x0.Length; k++)
                {
                    if (Math.Abs(a[j,k]) > 0.0000001)
                    {
                        notNullElementsIndexes.Add(k);
                    }
                }

                constraints.Add(new LinearConstraint(notNullElements.Length)
                {
                    VariablesAtIndices = notNullElementsIndexes.ToArray(),
                    CombinedAs = notNullElements,
                    ShouldBe = ConstraintType.EqualTo,
                    Value = b[j]
                });
            }

            var solver = new GoldfarbIdnani(func, constraints);
            if (!solver.Minimize()) throw new ApplicationException("Failed to solve balance task.");

            DisbalanceOriginal = a.Dot(x0).Subtract(b).Euclidean();
            Disbalance = a.Dot(solver.Solution).Subtract(b).Euclidean();

            return solver.Solution;
        }
    }
}
