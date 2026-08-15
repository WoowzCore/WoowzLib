using WLI_Render;
using WLO.Math;

namespace WLO.Render;

public class VKRenderView : WLI_Render.RenderView{
    public RenderContext Context{ get; }
    public Vector2I Viewport{ get; set; }
    public Color4B[] Get(){
        throw new NotImplementedException();
    }
}