using SynapticSharp.Calculation;
using SynapticSharp.NeuronProperties;
using System;
using System.Collections.Generic;

namespace SynapticSharp
{
    public class Neuron
    {
        protected int _id = NeuronIdentification.Uid;
        protected object _label = null;
        protected NeuronConnectionSet _connections = new NeuronConnectionSet();
        protected NeuronError _error = new NeuronError();
        protected NeuronTrace _trace = new NeuronTrace();
        protected double _state = 0;
        protected double _old = 0;
        protected double _activation = 0;
        protected Synapse _selfconnection;
        protected Func<double, bool, double> _squash = SquashFunctions.Logistic;
        protected List<Neuron> _neighbors = new List<Neuron>();
        protected double _bias = NeuronIdentification.Rng.NextDouble() * .2 - .1;
        protected double _derivative;

        public int Id => _id;
        public NeuronConnectionSet Connections => _connections;
        public NeuronError Error => _error;
        public NeuronTrace Trace => _trace;
        public double State => _state;
        public double Old => _old;
        public double Activation => _activation;
        public double Bias => _bias;
        public List<Neuron> Neighbors => _neighbors;
        public Synapse SelfConnection => _selfconnection;

        public Neuron()
        {
            _selfconnection = new Synapse(this, this, 0);
        }

        public double Activate()
        {
            //old state
            _old = _state;

            //eq. 15
            _state = _selfconnection.Gain * _selfconnection.Weight * _state + _bias;

            for (var i = 0; i < _connections.Inputs.Count; i++)
            {
                var input = _connections.Inputs[i];
                _state += input.Source._activation * input.Weight * input.Gain;
            }

            //eq. 16
            _activation = _squash(_state, false);

            //f'(s)
            _derivative = _squash(_state, true);

            //update traces
            var influences = new Dictionary<int, double>();
            for (var i = 0; i < _trace.Extended.Keys.Count; i++)
            {
                // extended elegibility trace
                var neuron = _neighbors[i];

                // if gated neuron's selfconnection is gated by this unit, the influence keeps track of the neuron's old state
                var influence = neuron._selfconnection.Gater == this ? neuron._old : 0;

                // index runs over all the incoming connections to the gated neuron that are gated by this unit
                for (var incoming = 0; incoming < _trace.Influences[neuron.Id].Count; incoming++)
                { // captures the effect that has an input connection to this unit, on a neuron that is gated by this unit
                    influence += _trace.Influences[neuron.Id][incoming].Weight *
                      _trace.Influences[neuron.Id][incoming].Source.Activation;
                }
                influences[neuron.Id] = influence;
            }

            for (var i = 0; i < _connections.Inputs.Count; i++)
            {
                var input = _connections.Inputs[i];

                // elegibility trace - Eq. 17
                _trace.Eligibility[input.Id] = _selfconnection.Gain * _selfconnection.Weight *
                    _trace.Eligibility[input.Id] + input.Gain * input.Source.Activation;

                for (var id = 0; i < _trace.Extended.Count; i++)
                {
                    // extended elegibility trace
                    var xtrace = _trace.Extended[id];
                    var neuron = Neighbors[id];
                    var influence = influences[neuron.Id];

                    // eq. 18
                    xtrace[input.Id] = neuron.SelfConnection.Gain * neuron.SelfConnection.Weight *
                        xtrace[input.Id] + _derivative * _trace.Eligibility[
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
            _derivative = 0;
            _bias = 0;
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
                _error.Responsibility = _error.Projected = target - _activation; // Eq. 10
            }
            else // the rest of the neuron compute their error responsibilities by backpropagation
            {
                // error responsibilities from all the connections pr+ojected from this neuron
                for (var id = 0; id < _connections.Projected.Count; id++)
                {
                    var connection = _connections.Projected[id];
                    var neuron = connection.Target;
                    // Eq. 21
                    error += neuron.Error.Responsibility * connection.Gain * connection.Weight;
                }

                // projected error responsibility
                _error.Projected = _derivative * error;

                error = 0;
                // error responsibilities from all the connections gated by this neuron
                for (var id = 0; id < _trace.Extended.Count; id++)
                {
                    var neuron = Neighbors[id]; // gated neuron
                    var influence = neuron.SelfConnection.Gater == this ? neuron.Old : 0; // if gated neuron's selfconnection is gated by this neuron

                    // index runs over all the connections to the gated neuron that are gated by this neuron
                    for (var input = 0; input < _trace.Influences[id].Count; input++)
                    { // captures the effect that the input connection of this neuron have, on a neuron which its input/s is/are gated by this neuron
                        influence += _trace.Influences[id][input].Weight * _trace.Influences[
                          neuron.Id][input].Source.Activation;
                    }
                    // eq. 22
                    error += neuron.Error.Responsibility * influence;
                }

                // gated error responsibility
                Error.Gated = _derivative * error;

                // error responsibility - Eq. 23
                Error.Responsibility = Error.Projected + Error.Gated;
            }

            // adjust all the neuron's incoming connections
            for (var id = 0; id < _connections.Inputs.Count; id++)
            {
                var input = _connections.Inputs[id];

                // Eq. 24
                var gradient = _error.Projected * _trace.Eligibility[input.Id];
                for (var id2 = 0; id2 < _trace.Extended.Count; id2++)
                {
                    var neuron = Neighbors[id2];
                    gradient += neuron.Error.Responsibility * _trace.Extended[neuron.Id][input.Id];
                }
                input.Weight += rate * gradient; // adjust weights - aka learn
            }

            // adjust bias
            _bias += rate * _error.Responsibility;
        }

        public Synapse Project(Neuron targetNeuron, double weight = 0)
        {
            // self-connection
            if (targetNeuron == this)
            {
                _selfconnection.Weight = 1;
                return _selfconnection;
            }

            Synapse connection; //the new connection

            // check if connection already exists
            var connected = Connected(targetNeuron);
            if (connected != null && connected.Type == ConnectedNeuronType.Projected)
            {
                // update connection
                if (weight > 0)
                    connected.Connection.Weight = weight;
                // return existing connection
                return connected.Connection;
            }
            else
            {
                // create a new connection
                connection = new Synapse(this, targetNeuron, weight);
            }

            // reference all the connections and traces
            _connections.Projected[connection.Id] = connection;
            _neighbors[targetNeuron.Id] = targetNeuron;
            targetNeuron.Connections.Inputs[connection.Id] = connection;
            targetNeuron.Trace.Eligibility[connection.Id] = 0;

            for (var id = 0; id < targetNeuron.Trace.Extended.Count; id++)
            {
                var trace = targetNeuron.Trace.Extended[id];
                trace[connection.Id] = 0;
            }

            return connection;
        }

        public void Gate(Synapse connection)
        {
            // add connection to gated list
            Connections.Gated[connection.Id] = connection;

            var neuron = connection.Target;
            if (!(Trace.Extended.ContainsKey(neuron.Id)))
            {
                // extended trace
                _neighbors[neuron.Id] = neuron;
                var xtrace = _trace.Extended[neuron.Id] = new Dictionary<int, double>();
                for (var id = 0; id < _connections.Inputs.Count; id++)
                {
                    var input = _connections.Inputs[id];
                    xtrace[input.Id] = 0;
                }
            }

            // keep track
            if (Trace.Influences.ContainsKey(neuron.Id))
            {
                _trace.Influences[neuron.Id].Add(connection);
            }
            else
            {
                _trace.Influences[neuron.Id] = new List<Synapse> { connection };
            }

            // set gater
            connection.Gater = this;
        }

        private ConnectedNeuronLocation Connected(Neuron targetNeuron)
        {
            var result = new ConnectedNeuronLocation();
            if (this == targetNeuron)
            {
                if (IsSelfConnected)
                {
                    result.Type = ConnectedNeuronType.SelfConnection;
                    result.Connection = SelfConnection;
                }
                else
                {
                    return null;
                }
            }

            var allConnections = new[] { Connections.Gated, Connections.Inputs, Connections.Projected };
            foreach (var allConnectionsOfType in allConnections)
            {
                var type = (ConnectedNeuronType?)(Array.IndexOf(allConnections, allConnectionsOfType) + 1);
                foreach (var connection in allConnectionsOfType)
                {
                    if (connection.Target == targetNeuron)
                    {
                        result.Type = type;
                        result.Connection = connection;
                        return result;
                    }
                    else if (connection.Source == targetNeuron)
                    {
                        result.Type = type;
                        result.Connection = connection;
                        return result;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Returns true or false, whether the neuron is self-connected or not
        /// </summary>
        public bool IsSelfConnected => _selfconnection.Weight > 0;
    }
}