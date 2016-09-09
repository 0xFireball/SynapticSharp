using System.Collections.Generic;

namespace SynapticSharp.NeuronProperties
{
    public class NeuronConnectionSet
    {
        public List<Synapse> Inputs { get; set; }
        public List<Synapse> Gated { get; set; }
        public List<Synapse> Projected { get; set; }
    }
}