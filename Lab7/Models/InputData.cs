namespace Lab7.Models
{
    public class InputData
    {
        public double[] X0 { get; set; }
        public double[,] A { get; set; }
        public double[] B { get; set; }
        public double[] Measurability { get; set; }
        public double[] Tolerance { get; set; }
        public double[] LowerMetrologic { get; set; }
        public double[] UpperMetrologic { get; set; }
        public double[] LowerTechnologic { get; set; }
        public double[] UpperTechnologic { get; set; }
        public string[] Names { get; set; }
        public string[] Guids { get; set; }
        public string[] NodesGuids { get; set; }
        public bool UseTechnologic { get; set; }
    }
}
