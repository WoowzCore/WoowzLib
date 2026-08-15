using WLI.GPU;
using WLO.Math;

namespace WLO.Render;

public class VKRenderContext : WLI_Render.RenderContext{
    public void Clear(Color4B Color){
        throw new NotImplementedException();
    }
    public Shader CurrentShader{ get; set; }
    public Mesh CurrentMesh{ get; set; }
    public void Draw(uint Count, uint Start = 0){
        throw new NotImplementedException();
    }
    public void DrawIndexed(uint Count, uint StartIndex = 0, int BaseVertex = 0){
        throw new NotImplementedException();
    }
}