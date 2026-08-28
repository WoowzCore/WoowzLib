using WLO.Math;

namespace WLI.GPU;

public interface Texture : WLI.GPU.Resource{
    Vector2I Size{ get; }
    void Update<T>(T[] Pixels, Rect2I? Rect = null) where T : unmanaged;
}