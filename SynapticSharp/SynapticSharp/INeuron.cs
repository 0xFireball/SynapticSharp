namespace SynapticSharp
{
    public interface INeuron
    {
        void Activate();
        void Propagate();
        void Project();
    }
}