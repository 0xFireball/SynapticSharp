namespace SynapticSharp
{
    public interface INeuron
    {
        double Activate();
        double Activate(double input);
        void Propagate(double rate, double target, bool isOutput);
        void Project();
    }
}