using SynapticSharp.NeuronProperties;

namespace SynapticSharp
{
    /// <summary>
    /// Represents a connection from one neuron to another
    /// </summary>
    public class Synapse
    {
        protected int _id = NeuronIdentification.Uid;
        protected Neuron _sourceNeuron;
        protected Neuron _targetNeuron;
        protected double _weight;
        private double _gain = 1;
        protected Neuron _gater = null;

        public int Id => _id;
        public Neuron Source => _sourceNeuron;
        public Neuron Target => _targetNeuron;
        public Neuron Gater => _gater;
        public double Gain { get { return _gain; } set { _gain = value; } }
        public double Weight { get { return _weight; } set { _weight = value; } }

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