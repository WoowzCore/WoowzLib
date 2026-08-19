using Silk.NET.OpenGL;
using WLI_Render;
using WLO.GPU;
using WLO.Math;
using WLO.Render.Hardware;

namespace WLO.Render;

public class GLRenderView : WLI.GPU.GLResource, WLI_Render.RenderView{
    private GLRenderView(OpenGL Render, Vector2I Size) : base(Render){
        Viewport = Size;

        ResultTexture = GLTexture2D.Create(__Owner, Size);

        ID = __Owner.API.GenFramebuffer();

        RenderView OldRenderView = __Owner.CRenderView;

        __Owner.CRenderView = this;

        __Owner.API.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, ResultTexture.ID, 0);

        if(__Owner.API.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != GLEnum.FramebufferComplete){
            throw new ExceptionWL("Произошла ошибка при создании GLRenderView! todo");
        }
        
        __Owner.CRenderView = OldRenderView;
    }
    
    public static GLRenderView Create(OpenGL Render, Vector2I Size){
        GLRenderView Result = new GLRenderView(Render, Size);
        
        Render.Registry_RenderView[Result.ID] = Result;

        return Result;
    }
    
    private GLRenderView(OpenGL Render, uint TargetID) : base(Render){
        FromID = true;
        ID = TargetID;

        if(ID == 0){
            unsafe{
                int* V = stackalloc int[4];
                __Owner.API.GetInteger(GetPName.Viewport, V);
                Viewport = new Vector2I(V[2], V[3]);
            }
            ResultTexture = null;
        }else{
            RenderView Old = __Owner.CRenderView;
        
            __Owner.API.GetFramebufferAttachmentParameter(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, GLEnum.FramebufferAttachmentObjectName, out int TextureID);

            if(TextureID > 0){
                ResultTexture = GLTexture2D.GetExists(__Owner, (uint)TextureID);
                Viewport = ResultTexture.Size;
            }
        
            __Owner.CRenderView = Old;
        }
    }

    public static GLRenderView GetExists(OpenGL Render, uint TargetID){
        if(Render.Registry_RenderView.TryGetValue(TargetID, out GLRenderView Result)){ return Result; }

        if(TargetID != 0 && !Render.API.IsFramebuffer(TargetID)){ throw new ExceptionWL("Указан несуществующий ID!"); }

        Result = new GLRenderView(Render, TargetID);
        Render.Registry_RenderView[TargetID] = Result;

        return Result;
    }
    
    public override void OnDestroy(){
        if(ID != 0){ __Owner.API.DeleteFramebuffer(ID); }
        ResultTexture?.Destroy();
    }
    
    // ----------------------------------------------------------------------
    
    public Vector2I Viewport{ get; set; }
    
    public WLI.GPU.Texture? ResultTexture{ get; private set; }
    
    public Color4B[] Get(){
        throw new NotImplementedException();
    }
}