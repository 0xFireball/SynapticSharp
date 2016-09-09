using System;

namespace SynapticSharp.NeuronProperties
{
    internal static class NeuronIdentification
    {
        private static int startIdentifier;
        private static Random rng;

        static NeuronIdentification()
        {
            startIdentifier = 0;
            rng = new Random();
        }

        public static int Uid
        {
            get
            {
                return ++startIdentifier;
            }
        }

        public static Random Rng
        {
            get
            {
                return rng;
            }
        }
    }
}