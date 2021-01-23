using System;
using System.Collections.Generic;

namespace Balance
{
    public interface IBalanceSolver
    {
        double DisbalanceOriginal { get; }
        double Disbalance { get; }
        public TimeSpan Time { get; }
        public TimeSpan TimeAll { get; }

        double[] Solve(double[] x0, double[,] a, double[] b, double[] measurability, double[] tolerance, double[] lower,
            double[] upper);
        double GlobalTest(double[] x0, double[,] a, double[] measurability, double[] tolerance);
        public double[,] GlrTest(double[] x0, double[,] a, double[] measurability, double[] tolerance,
            IEnumerable<(int, int, int)> flows, double globalTest);
        public IEnumerable<(int, int, int)> GetFlows(double[,] a);
    }
}
