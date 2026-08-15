using WLI_Render;
using WLO.Math;
using WLO.Render.Hardware;

namespace WLO.Render;

public class GLRenderView : WLI_Render.RenderView{
    public RenderContext Context{ get; }
    
    public Vector2I Viewport{ get; set; }

    public GLRenderView(OpenGL Render) => Context = new GLRenderContext(Render);
    
    public Color4B[] Get(){
        throw new NotImplementedException();
    }
}