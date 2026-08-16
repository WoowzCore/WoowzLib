using WLI_Render;
using WLO.Math;
using WLO.Render.Hardware;

namespace WLO.Render;

public class GLRenderView : WLI.GPU.GLResource, WLI_Render.RenderView{
    public RenderContext Context{ get; }
    
    public Vector2I Viewport{ get; set; }

    // ----------------------------------------------------------------------
    
    public WLI.GPU.Texture? ResultTexture{ get; private set; }
    
    public GLRenderView(OpenGL Render) : base(Render){
        Context = new GLRenderContext(Render);
        ID = 0;
    }

    public GLRenderView(OpenGL Render, Vector2I Size) : base(Render){
        Context = new GLRenderContext(Render);
        
    }
    
    // ----------------------------------------------------------------------
    
    public Color4B[] Get(){
        throw new NotImplementedException();
    }
    public override void Dispose(){
        if(ID != 0){ __Owner.API.DeleteFramebuffer(ID); }
        ResultTexture?.Dispose();
    }
}