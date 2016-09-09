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

        public static Func<double, bool, double> Tanh = (x, derivate) =>
        {
            if (derivate)
                return 1 - Math.Pow(Tanh(x, false), 2);
            var eP = Math.Exp(x);
            var eN = 1 / eP;
            return (eP - eN) / (eP + eN);
        };

        public static Func<double, bool, double> Identity = (x, derivate) =>
        {
            return derivate ? 1 : x;
        };

        public static Func<double, bool, double> HLIM = (x, derivate) =>
        {
            return derivate ? 1 : x > 0 ? 1 : 0;
        };

        public static Func<double, bool, double> RELU = (x, derivate) =>
        {
            if (derivate)
            {
                return x > 0 ? 1 : 0;
            }
            return x > 0 ? x : 0;
        };
    }
}