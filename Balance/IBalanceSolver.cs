namespace Balance
{
    public interface IBalanceSolver
    {
        double DisbalanceOriginal { get; }
        double Disbalance { get; }

        double[] Solve(double[] x0, double[,] a, double[] b, double[] measurability, double[] tolerance, double[] lower,
            double[] upper);
    }
}
