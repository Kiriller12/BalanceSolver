namespace Lab7.Models
{
    public class InputData
    {
        public double[] X0 { get; set; }
        public double[,] A { get; set; }
        public double[] B { get; set; }
        public double[] Measurability { get; set; }
        public double[] Tolerance { get; set; }
        public double[] Lower { get; set; }
        public double[] Upper { get; set; }
    }
}
