using WLO.Math;

namespace WLI.GPU;

public interface Texture : WLI.GPU.Resource{
    Vector2I Size{ get; }
    void SetData<T>(T[] Pixels) where T : unmanaged;
}