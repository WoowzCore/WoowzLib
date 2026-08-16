using WLO.Math;

namespace WLI_Render;

public interface RenderContext{
    WLI.GPU.Program? CurrentProgram{ set; get; }
    WLI.GPU.Mesh?    CurrentMesh  { set; get; }
    
    void Clear(Color4B Color);
    
    void Draw(uint Count, uint Start = 0);
    void DrawIndexed(uint Count, uint StartIndex = 0, int BaseVertex = 0);
}