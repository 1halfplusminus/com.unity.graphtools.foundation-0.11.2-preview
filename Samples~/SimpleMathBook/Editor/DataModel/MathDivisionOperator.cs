using System;
using System.Linq;

namespace UnityEditor.GraphToolsFoundation.Overdrive.Samples.MathBook
{
    [Serializable]
    public class MathDivisionOperator : MathOperator
    {
        public override string Title
        {
            get => "Divide";
            set { }
        }

        public override float Evaluate()
        {
            return Values.Skip(1).Aggregate(Values.FirstOrDefault(), (current, value) => current / value);
        }

        protected override void AddInputPorts()
        {
            for (var i = 0; i < InputPortCount; ++i)
                this.AddDataInputPort<float>(i == 0 ? "Dividend" : "Divisor " + i);
        }
    }
}
