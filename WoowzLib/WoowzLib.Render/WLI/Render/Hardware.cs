using WLI.GPU;
using WLO;
using WLO.Math;

namespace WLI_Render;

// Рендер векторами/полигонами
public interface Hardware : WLI.Render, WLI.Engine{
    RenderView CurrentRenderView{ get; }
    
    void FrameStart(RenderView? Target = null);
    void FrameStop();
    
    WLI.GPU.Buffer CreateBuffer(uint Usage, uint Size);
    WLI.GPU.Texture CreateTexture(Vector2I Size, uint Format);
    WLI.GPU.Shader CreateShader(WLI.GPU.Shader.Type Stage, string Source);
    WLI.GPU.Program CreateProgram(params WLI.GPU.Shader[] Shaders);
    WLI.GPU.Mesh CreateMesh<T>(VertexLayout Layout, T[] Vertices, uint[]? Indices = null) where T : unmanaged;
}