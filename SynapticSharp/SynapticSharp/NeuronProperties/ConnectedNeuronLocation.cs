namespace SynapticSharp.NeuronProperties
{
    internal class ConnectedNeuronLocation
    {
        public ConnectedNeuronType? Type { get; set; }
        public Synapse Connection { get; set; }
    }
}