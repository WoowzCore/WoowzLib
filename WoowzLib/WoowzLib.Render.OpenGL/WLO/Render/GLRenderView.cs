using Silk.NET.OpenGL;
using WLI_Render;
using WLO.GPU;
using WLO.Math;
using WLO.Render.Hardware;

namespace WLO.Render;

public class GLRenderView : WLI.GPU.GLResource, WLI_Render.RenderView{
    public Vector2I Viewport{ get; set; }

    // ----------------------------------------------------------------------
    
    public WLI.GPU.Texture? ResultTexture{ get; private set; }
    
    public GLRenderView(OpenGL Render) : base(Render){
        ID = 0; // Это типо Framebuffer окна
    }

    public GLRenderView(OpenGL Render, Vector2I Size) : base(Render){
        Viewport = Size;

        ResultTexture = new GLTexture(__Owner, Size);

        ID = __Owner.API.GenFramebuffer();

        RenderView OldRenderView = __Owner.CRenderView;

        __Owner.CRenderView = this;

        __Owner.API.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, ResultTexture.ID, 0);

        if(__Owner.API.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != GLEnum.FramebufferComplete){
            throw new ExceptionWL("Произошла ошибка при создании GLRenderView!");
        }
        
        __Owner.CRenderView = OldRenderView;
    }
    
    // ----------------------------------------------------------------------
    
    public Color4B[] Get(){
        throw new NotImplementedException();
    }
    
    public override void OnDestroy(){
        if(ID != 0){ __Owner.API.DeleteFramebuffer(ID); }
        ResultTexture?.Destroy();
    }
}