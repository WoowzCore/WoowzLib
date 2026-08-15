namespace WLI.GPU;

public interface Buffer : WLI.GPU.Resource{
    uint Size{ get; }
    void Update<T>(T[] Data) where T : unmanaged;
}