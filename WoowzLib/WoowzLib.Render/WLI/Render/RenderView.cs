using WLO;
using WLO.Math;

namespace WLI_Render;

public interface RenderView{
    Vector2I Viewport{ get; set; }
    
    Color4B[] Get();
}