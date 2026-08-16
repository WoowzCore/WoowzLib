using WLO;
using WLO.Math;

namespace WLI_Render;

public interface RenderView{
    WLI_Render.RenderContext Context{ get; }
    
    Vector2I Viewport{ get; set; }
    
    Color4B[] Get();
}