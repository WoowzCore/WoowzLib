namespace WLO.GPU;

public class VKBuffer : WLI.GPU.Buffer{
    public void Dispose(){
        // TODO release managed resources here
    }
    public uint ID{ get; }
    public uint Size{ get; }
    public void Update<T>(T[] Data) where T : unmanaged{
        throw new NotImplementedException();
    }
}