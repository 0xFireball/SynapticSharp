using SynapticSharp.NeuronProperties;

namespace SynapticSharp
{
    /// <summary>
    /// Represents a connection from one neuron to another
    /// </summary>
    public class Synapse : ISynapse
    {
        protected int _id = NeuronIdentification.Uid;
        protected Neuron _sourceNeuron;
        protected Neuron _targetNeuron;
        protected double _weight;
        protected double _gain = 1;
        protected object _gater = null;

        public Synapse(Neuron sourceNeuron, Neuron targetNeuron)
        {
            Initialize(sourceNeuron, targetNeuron, NeuronIdentification.Rng.NextDouble() * .2 - .1);
        }

        public Synapse(Neuron sourceNeuron, Neuron targetNeuron, double weight)
        {
            Initialize(sourceNeuron, targetNeuron, weight);
        }

        private void Initialize(Neuron sourceNeuron, Neuron targetNeuron, double weight)
        {
            _sourceNeuron = sourceNeuron;
            _targetNeuron = targetNeuron;
            _weight = weight;
        }
    }
}