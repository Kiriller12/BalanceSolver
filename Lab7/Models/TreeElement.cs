using System;
using System.Collections.Generic;

namespace Lab7.Models
{
    public class TreeElement
    {
        public TreeElement()
        {
        }

        public TreeElement(List<(int, int)> flows, double testValue)
        {
            Flows = flows;
            TestValue = testValue;
        }

        public Guid Id { get; } = Guid.NewGuid();

        public List<(int, int)> Flows { get; } = new List<(int, int)>();

        public double TestValue { get; }
    }
}
