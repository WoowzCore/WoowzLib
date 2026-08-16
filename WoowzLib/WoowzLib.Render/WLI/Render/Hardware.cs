using WLI.GPU;
using WLO;
using WLO.Math;
using Buffer = WLI.GPU.Buffer;

namespace WLI_Render;

// Рендер векторами/полигонами
public interface Hardware : WLI.Render, WLI.Engine{
    void FrameStart();
    void FrameStop();
    
    // ----------------------------------------------------------------------
    
    WLI.GPU.Buffer CreateBuffer(uint Usage, uint Size);
    WLI.GPU.Texture CreateTexture(Vector2I Size, uint Format);
    WLI.GPU.Shader CreateShader(WLI.GPU.Shader.Type Stage, string Source);
    WLI.GPU.Program CreateProgram(params WLI.GPU.Shader[] Shaders);
    WLI.GPU.Mesh CreateMesh<T>(VertexLayout Layout, T[] Vertices, uint[]? Indices = null) where T : unmanaged;
    
    // ----------------------------------------------------------------------
    
    RenderView CRenderView{ get; }
    Mesh? CMesh{ get; }
    Program? CProgram{ get; }
    Buffer? CFBuffer{ get; }
    Buffer? CIBuffer{ get; }
    Texture? CTexture{ get; }
    
    // ----------------------------------------------------------------------
    
    void Clear(Color4B Color);
    
    void Draw(uint Count, uint Start = 0);
    void DrawIndexed(uint Count, uint StartIndex = 0, int BaseVertex = 0);
    void Draw(Mesh Mesh, Program? Program);
    void Draw(Mesh Mesh);
}