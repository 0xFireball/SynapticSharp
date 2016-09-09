using SynapticSharp.Calculation;
using SynapticSharp.NeuronProperties;
using System;
using System.Collections.Generic;

namespace SynapticSharp
{
    public class Neuron : INeuron
    {
        protected int _id = NeuronIdentification.Uid;
        protected object _label = null;
        protected NeuronConnectionSet _connections = new NeuronConnectionSet();
        protected NeuronError _error = new NeuronError();
        protected NeuronTrace trace = new NeuronTrace();
        protected double _state = 0;
        protected double _old = 0;
        protected double _activation = 0;
        protected Synapse _selfconnection;
        protected Func<double, bool, double> _squash = SquashFunctions.Logistic;
        protected List<Neuron> _neighbors = new List<Neuron>();
        protected double _bais = NeuronIdentification.Rng.NextDouble() * .2 - .1;

        public int Id => _id;
        public double State => _state;
        public double Activation => _activation;
        public double Bias => _bais;
        public List<Neuron> Neighbors => _neighbors;
        public Synapse SelfConnection => _selfconnection;

        protected double derivative;

        public Neuron()
        {
            _selfconnection = new Synapse(this, this, 0);
        }

        public double Activate()
        {
            //old state
            _old = _state;

            //eq. 15
            _state = _selfconnection.Gain * _selfconnection.Weight * _state + _bais;

            for (var i = 0; i < _connections.Inputs.Count; i++)
            {
                var input = _connections.Inputs[i];
                _state += input.Source._activation * input.Weight * input.Gain;
            }

            //eq. 16
            _activation = _squash(_state, false);

            //f'(s)
            derivative = _squash(_state, true);

            //update traces
            var influences = new Dictionary<int, double>();
            for (var i = 0; i < trace.Extended.Keys.Count; i++)
            {
                // extended elegibility trace
                var neuron = _neighbors[i];

                // if gated neuron's selfconnection is gated by this unit, the influence keeps track of the neuron's old state
                var influence = neuron._selfconnection.Gater == this ? neuron._old : 0;

                // index runs over all the incoming connections to the gated neuron that are gated by this unit
                for (var incoming = 0; incoming < trace.Influences[neuron.Id].Count; incoming++)
                { // captures the effect that has an input connection to this unit, on a neuron that is gated by this unit
                    influence += trace.Influences[neuron.Id][incoming].Weight *
                      trace.Influences[neuron.Id][incoming].Source.Activation;
                }
                influences[neuron.Id] = influence;
            }

            for (var i = 0; i < _connections.Inputs.Count; i++)
            {
                var input = _connections.Inputs[i];

                // elegibility trace - Eq. 17
                trace.Eligibility[input.Id] = _selfconnection.Gain * _selfconnection.Weight *
                    trace.Eligibility[input.Id] + input.Gain * input.Source.Activation;

                for (var id = 0; i < trace.Extended.Count; i++)
                {
                    // extended elegibility trace
                    var xtrace = trace.Extended[id];
                    var neuron = Neighbors[id];
                    var influence = influences[neuron.Id];

                    // eq. 18
                    xtrace[input.Id] = neuron.SelfConnection.Gain * neuron.SelfConnection.Weight *
                        xtrace[input.Id] + derivative * trace.Eligibility[
                        input.Id] * influence;
                }
            }

            //  update gated connection's gains
            for (var connection = 0; connection < _connections.Gated.Count; connection++)
            {
                _connections.Gated[connection].Gain = _activation;
            }

            return _activation;
        }

        public double Activate(double input)
        {
            _activation = input;
            derivative = 0;
            _bais = 0;
            return _activation;
        }

        /// <summary>
        /// Backpropagate the error.
        /// </summary>
        /// <param name="rate"></param>
        /// <param name="target"></param>
        public void Propagate(double rate, double target, bool isOutput)
        {
            // error accumulator
            var error = 0d;

            // whether or not this neuron is in the output layer

            // output neurons get their error from the enviroment
            if (isOutput)
            {
                _error.Responsibility = _error.Projected = target - this.Activation; // Eq. 10
            }
        }

        public void Project()
        {
            throw new NotImplementedException();
        }
    }
}