namespace WLI.GPU;

public interface Buffer : WLI.GPU.Resource{
    uint Size{ get; }
    
    void Update<T>(ReadOnlySpan<T> Data, uint Offset = 0) where T : unmanaged;
    void Update<T>(T[] Data, uint Offset = 0) where T : unmanaged;
    void Read<T>(Span<T> Destination, uint Offset = 0) where T : unmanaged;
}