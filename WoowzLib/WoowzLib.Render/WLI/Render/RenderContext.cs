using WLO.Math;

namespace WLI_Render;

public interface RenderContext{
    void Clear(Color4B Color);
    
    WLI.GPU.Shader CurrentShader{ set; get; }
    WLI.GPU.Mesh   CurrentMesh  { set; get; }
    
    void Draw(uint Count, uint Start = 0);
    void DrawIndexed(uint Count, uint StartIndex = 0, int BaseVertex = 0);
}