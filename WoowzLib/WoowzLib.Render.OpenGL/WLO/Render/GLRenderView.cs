using Silk.NET.OpenGL;
using WLI_Render;
using WLO.GPU;
using WLO.Math;
using WLO.Render.Hardware;
using Texture = WLI.GPU.Texture;

namespace WLO.Render;

public class GLRenderView : WLI.GPU.GLResource, WLI_Render.RenderView{
    private readonly Dictionary<FramebufferAttachment, WLI.GPU.Texture> __Textures      = [];
    private readonly List<uint>                                         __RenderBuffers = [];
    
    private GLRenderView(OpenGL Render, Vector2I Size, params LayerConfig[] Layers) : base(Render){
        Viewport = Size;
        ID = __Owner.API.GenFramebuffer();
        
        RenderView OldRenderView = __Owner.CRenderView;
        __Owner.CRenderView = this;

        List<GLEnum> ColorAttachments = [];

        foreach(LayerConfig Layer in Layers){
            if(Layer.IsTexture){
                PixelFormat CPUFormat = PixelFormat.Rgba;
                PixelType   Type      = PixelType.UnsignedByte;

                if(Layer.Attachment == FramebufferAttachment.DepthAttachment){
                    CPUFormat = PixelFormat.DepthComponent;
                    Type = PixelType.Float;
                }else if(Layer.Attachment == FramebufferAttachment.StencilAttachment){
                    CPUFormat = PixelFormat.StencilIndex;
                    Type = PixelType.UnsignedByte;
                }else if(Layer.Attachment == FramebufferAttachment.DepthStencilAttachment){
                    CPUFormat = PixelFormat.DepthStencil;
                    Type = PixelType.UnsignedInt248;
                }
                
                GLTexture2D Texture = GLTexture2D.Create(__Owner, Size, Layer.Format, CPUFormat, Type);
                __Owner.API.FramebufferTexture2D(FramebufferTarget.Framebuffer, Layer.Attachment, TextureTarget.Texture2D, Texture.ID, 0);
                __Textures[Layer.Attachment] = Texture;

                if(Layer.Attachment >= FramebufferAttachment.ColorAttachment0 && Layer.Attachment <= FramebufferAttachment.ColorAttachment31){
                    ColorAttachments.Add((GLEnum)Layer.Attachment);
                }
            }else{
                // todo: (вынести как отдельный класс???)
                uint RenderBuffer = __Owner.API.GenRenderbuffer();
                __Owner.API.BindRenderbuffer(RenderbufferTarget.Renderbuffer, RenderBuffer);
                __Owner.API.RenderbufferStorage(RenderbufferTarget.Renderbuffer, Layer.Format, (uint)Size.X, (uint)Size.Y);
                __Owner.API.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, Layer.Attachment, RenderbufferTarget.Renderbuffer, RenderBuffer);
                __RenderBuffers.Add(RenderBuffer);
            }
        }

        if(ColorAttachments.Count > 0){
            unsafe{
                fixed(GLEnum* Buffers = ColorAttachments.ToArray()){
                    __Owner.API.DrawBuffers((uint)ColorAttachments.Count, Buffers);
                }
            }
        }else{
            __Owner.API.DrawBuffer(DrawBufferMode.None);
            __Owner.API.ReadBuffer(ReadBufferMode.None);
        }

        if(__Owner.API.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != GLEnum.FramebufferComplete){
            throw new ExceptionWL("todo, framebuffer error");
        }
        
        __Owner.CRenderView = OldRenderView;
    }
    
    public static GLRenderView Create(OpenGL Render, Vector2I Size, params LayerConfig[] Layers){
        if(Layers.Length == 0){ Layers = [ LayerConfig.Color(), LayerConfig.Depth() ]; }

        GLRenderView Result = new GLRenderView(Render, Size, Layers);
        
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
        }else{
            RenderView Old = __Owner.CRenderView;
            __Owner.CRenderView = this;

            foreach(object? __FramebufferAttachment in Enum.GetValues(typeof(FramebufferAttachment))){
                FramebufferAttachment Attachment = (FramebufferAttachment)__FramebufferAttachment;

                __Owner.API.GetFramebufferAttachmentParameter(FramebufferTarget.Framebuffer, Attachment, FramebufferAttachmentParameterName.ObjectType, out int Type);
                if(Type == (int)GLEnum.None){ continue; }

                __Owner.API.GetFramebufferAttachmentParameter(FramebufferTarget.Framebuffer, Attachment, FramebufferAttachmentParameterName.ObjectName, out int ObjectID);
                if(ObjectID <= 0){ continue; }

                if(Type == (int)GLEnum.Texture){
                    GLTexture2D Texture = GLTexture2D.GetExists(__Owner, (uint)ObjectID);
                    __Textures[Attachment] = Texture;
                    if(Attachment == FramebufferAttachment.ColorAttachment0){ Viewport = Texture.Size; }
                }else if(Type == (int)GLEnum.Renderbuffer){
                    __RenderBuffers.Add((uint)ObjectID);
                }
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
        __Owner.Registry_RenderView.Remove(ID);
        if(ID != 0){ __Owner.API.DeleteFramebuffer(ID); }

        foreach(uint RenderBuffer in __RenderBuffers){ __Owner.API.DeleteRenderbuffer(RenderBuffer); }
        foreach(Texture Texture in __Textures.Values){ Texture.Destroy(); }
        
        __Textures.Clear();
        __RenderBuffers.Clear();
    }
    
    // ----------------------------------------------------------------------
    
    public Vector2I Viewport{ get; set; }

    public Texture? TextureColor0 => GetTexture(FramebufferAttachment.ColorAttachment0);
    public Texture? TextureDepth  => GetTexture(FramebufferAttachment.DepthAttachment);

    public Texture? GetTexture(FramebufferAttachment Attachment) => __Textures.TryGetValue(Attachment, out Texture? Texture) ? Texture : null;
    
    public Color4B[] Get(){
        Color4B[] Pixels = new Color4B[Viewport.X * Viewport.Y];
        RenderView Old = __Owner.CRenderView;
        __Owner.CRenderView = this;

        unsafe{
            fixed(void* Ptr = Pixels){
                __Owner.API.ReadPixels(0, 0, (uint)Viewport.X, (uint)Viewport.Y, PixelFormat.Rgba, PixelType.UnsignedByte, Ptr);
            }
        }
        
        __Owner.CRenderView = Old;
        return Pixels;
    }
    
    // ----------------------------------------------------------------------
    
    public struct LayerConfig{
        public FramebufferAttachment Attachment;
        public InternalFormat        Format;
        public bool                  IsTexture;

        public static LayerConfig Color(int Index = 0, bool Texture = true) => new LayerConfig{
            Attachment = FramebufferAttachment.ColorAttachment0 + Index,
            Format = InternalFormat.Rgba8,
            IsTexture = Texture
        };
        public static LayerConfig Depth(bool Texture = false) => new LayerConfig{
            Attachment = FramebufferAttachment.DepthAttachment,
            Format = InternalFormat.DepthComponent24,
            IsTexture = Texture
        };
        public static LayerConfig Stencil(bool Texture = false) => new LayerConfig{
            Attachment = FramebufferAttachment.StencilAttachment,
            Format = InternalFormat.StencilIndex8,
            IsTexture = Texture
        };
        public static LayerConfig DepthStencil(bool Texture = false) => new LayerConfig{
            Attachment = FramebufferAttachment.DepthStencilAttachment,
            Format = InternalFormat.Depth24Stencil8,
            IsTexture = Texture
        };
    }
}