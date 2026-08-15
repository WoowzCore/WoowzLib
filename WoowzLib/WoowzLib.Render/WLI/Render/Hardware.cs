using WLO;
using WLO.Math;

namespace WLI_Render;

// Рендер векторами/полигонами
public interface Hardware : WLI.Render, WLI.Engine{
    WLI.GPU.Buffer CreateBuffer(uint Usage, uint Size);
    WLI.GPU.Texture CreateTexture(Vector2I Size, uint Format);
    WLI.GPU.Shader CreateShader(string VertexSource, string FragmentSource);
    WLI.GPU.Mesh CreateMesh<T>(T[] Vertices, uint[]? Indices = null) where T : unmanaged;
    
    RenderView CurrentRenderView{ get; }
    
    void FrameStart(RenderView? Target = null);
    void FrameStop();
}