using System.Collections.Generic;

namespace Lab7.Models
{
    public class InputGraph
    {
        public BalanceSettings BalanceSettings { get; set; }
        public object Dependencies { get; set; }
        public List<Variable> Variables { get; set; }
    }
}
