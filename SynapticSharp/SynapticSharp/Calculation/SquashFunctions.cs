using System;

namespace SynapticSharp.Calculation
{
    public class SquashFunctions
    {
        public static Func<double, bool, double> Logistic = (x, derivate) =>
        {
            if (!derivate)
            {
                return 1 / (1 + Math.Exp(-x));
            }
            var fx = Logistic(x, false);
            return fx * (1 - fx);
        };


    }
}