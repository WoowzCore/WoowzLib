using WLO.Math;

namespace WLO.GPU;

public class VKTexture : WLI.GPU.Texture{
    public void Dispose(){
        throw new NotImplementedException();
    }
    public uint ID{ get; }
    public Vector2I Size{ get; }
    public void SetData<T>(T[] Pixels) where T : unmanaged{
        throw new NotImplementedException();
    }
}