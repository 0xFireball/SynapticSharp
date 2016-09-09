using System.Collections.Generic;

namespace SynapticSharp.NeuronProperties
{
    public class NeuronTrace
    {
        public Dictionary<int, Dictionary<int, double>> Extended = new Dictionary<int, Dictionary<int, double>>();
        public Dictionary<int, List<Synapse>> Influences = new Dictionary<int, List<Synapse>>();
        public Dictionary<int, double> Eligibility = new Dictionary<int, double>();
    }
}