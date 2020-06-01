namespace Lab7.Models
{
    public class BalanceSettings
    {
        public bool AltPointBoundsFlag { get; set; }
        public string BoundsType { get; set; }
        public string ConstrFeasAnalysis { get; set; }
        public bool ConstrSatisfactionFlag { get; set; }
        public int DefaultAbsTolerance { get; set; }
        public double DefaultTechnologicalLowerBoundValue { get; set; }
        public double DefaultTechnologicalUpperBoundValue { get; set; }
        public double FixUnit { get; set; }
        public bool FixValueSolution { get; set; }
        public bool GlobalTestOnly { get; set; }
        public double MaxImbalanceValue { get; set; }
        public int MaxRoundingEffortsParameter { get; set; }
        public int MaxSolverIterations { get; set; }
        public string PenaltyType { get; set; }
        public double RoundUnit { get; set; }
        public bool RoundValueSolution { get; set; }
        public string WeightsType { get; set; }
    }
}
