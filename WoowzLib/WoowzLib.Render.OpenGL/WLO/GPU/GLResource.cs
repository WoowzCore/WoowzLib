using WLO.Render.Hardware;

namespace WLO.GPU;

public abstract class GLResource : WLI.GPU.Resource{
    public uint ID{ get; protected set; }
    
    protected OpenGL __Owner;

    protected GLResource(OpenGL Render) => __Owner = Render;
    
    public abstract void Dispose();
}