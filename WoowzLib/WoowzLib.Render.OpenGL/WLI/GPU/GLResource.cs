using WLO.Render.Hardware;

namespace WLI.GPU;

public abstract class GLResource : WLI.GPU.Resource{
    public uint ID{ get; protected set; }
    public bool FromID{ get; protected set; }
    
    protected OpenGL __Owner;

    protected GLResource(OpenGL Render) => __Owner = Render;

    public bool Destroyed{ get; set; }

    public void Destroy() => ((Destroyable)this).Destroy();
    public void Dispose() => Destroy();

    public abstract void OnDestroy();
}