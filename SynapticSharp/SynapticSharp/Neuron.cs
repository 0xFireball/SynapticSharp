using SynapticSharp.NeuronProperties;
using System;

namespace SynapticSharp
{
    public abstract class Neuron : INeuron
    {
        protected int id = NeuronIdentification.Uid;
        protected object label = null;
        protected NeuronConnectionSet connections = new NeuronConnectionSet();
        protected NeuronError error = new NeuronError();
        protected NeuronTrace trace = new NeuronTrace();
        protected double state = 0;
        protected double old = 0;
        protected double activation = 0;
        protected Synapse selfconnection;

        public Neuron()
        {
            selfconnection = new Synapse(this, this, 0);
        }

        public void Activate()
        {
            throw new NotImplementedException();
        }

        public void Propagate()
        {
            throw new NotImplementedException();
        }

        public void Project()
        {
            throw new NotImplementedException();
        }
    }
}