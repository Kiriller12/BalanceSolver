namespace Lab7.Models
{
    public class Variable
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string SourceId { get; set; }
        public string DestinationId { get; set; }
        public bool IsExcluded { get; set; }
        public Range MetrologicRange { get; set; }
        public Range TechnologicRange { get; set; }
        public double Tolerance { get; set; }
        public double Measured { get; set; }
        public bool IsMeasured { get; set; }
        public bool InService { get; set; }
        public bool ExactRounding { get; set; }
        public string VarType { get; set; }
    }
}
