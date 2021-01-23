using System.Collections.Generic;

namespace Lab7.Models
{
    public class GlrOutput
    {
        public List<GlrOutputFlow> FlowsInfo { get; set; }

        public List<Variable> FlowsToAdd { get; set; }

        public double TestValue { get; set; }
    }
}
