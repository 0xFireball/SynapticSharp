namespace SynapticSharp
{
    public interface INeuron
    {
        double Activate();
        double Activate(double input);
        void Propagate();
        void Project();
    }
}