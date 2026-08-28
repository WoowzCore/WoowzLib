using Silk.NET.OpenGL;
using WLI_Render;
using WLO.GPU;
using WLO.Math;
using WLO.Render.Hardware;
using Texture = WLI.GPU.Texture;

namespace WLO.Render;

public class GLView : WLI.GPU.GLResource, WLI_Render.View{
    private readonly Dictionary<FramebufferAttachment, GLTexture2D> __Textures      = [];
    private readonly List      <uint                              > __RenderBuffers = [];
    
    private GLView(OpenGL Render, Vector2I Size, params LayerConfig[] Layers) : base(Render){
        Viewport = Size;
        ID = Owner.API.GenFramebuffer();
        
        GLView OldView = Owner.Pool.GetView();
        Owner.Pool.SetView(this, true);

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
                
                GLTexture2D Texture = GLTexture2D.Create(Owner, Size, Layer.Format, CPUFormat, Type);
                Owner.API.FramebufferTexture2D(FramebufferTarget.Framebuffer, Layer.Attachment, TextureTarget.Texture2D, Texture.ID, 0);
                __Textures[Layer.Attachment] = Texture;

                if(Layer.Attachment >= FramebufferAttachment.ColorAttachment0 && Layer.Attachment <= FramebufferAttachment.ColorAttachment31){
                    ColorAttachments.Add((GLEnum)Layer.Attachment);
                }
            }else{
                // todo: (вынести как отдельный класс???)
                uint RenderBuffer = Owner.API.GenRenderbuffer();
                Owner.API.BindRenderbuffer(RenderbufferTarget.Renderbuffer, RenderBuffer);
                Owner.API.RenderbufferStorage(RenderbufferTarget.Renderbuffer, Layer.Format, (uint)Size.X, (uint)Size.Y);
                Owner.API.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, Layer.Attachment, RenderbufferTarget.Renderbuffer, RenderBuffer);
                __RenderBuffers.Add(RenderBuffer);
            }
        }

        if(ColorAttachments.Count > 0){
            unsafe{
                fixed(GLEnum* Buffers = ColorAttachments.ToArray()){
                    Owner.API.DrawBuffers((uint)ColorAttachments.Count, Buffers);
                }
            }
        }else{
            Owner.API.DrawBuffer(DrawBufferMode.None);
            Owner.API.ReadBuffer(ReadBufferMode.None);
        }

        if(Owner.API.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != GLEnum.FramebufferComplete){
            throw new ExceptionWL("todo, framebuffer error");
        }
        
        Owner.Pool.SetView(OldView, true);;
    }
    
    public static GLView Create(OpenGL Render, Vector2I Size, params LayerConfig[] Layers){
        if(Layers.Length == 0){ Layers = [ LayerConfig.Color(), LayerConfig.Depth() ]; }

        GLView Result = new GLView(Render, Size, Layers);
        
        Render.Pool.RegistryView[Result.ID] = Result;

        return Result;
    }
    
    private GLView(OpenGL Render, uint TargetID) : base(Render){
        FromID = true;
        ID = TargetID;

        if(ID == 0){
            unsafe{
                int* V = stackalloc int[4];
                Owner.API.GetInteger(GetPName.Viewport, V);
                Viewport = new Vector2I(V[2], V[3]);
            }
        }else{
            GLView OldView = Owner.Pool.GetView();
            Owner.Pool.SetView(this, true);

            foreach(object? __FramebufferAttachment in Enum.GetValues(typeof(FramebufferAttachment))){
                FramebufferAttachment Attachment = (FramebufferAttachment)__FramebufferAttachment;

                Owner.API.GetFramebufferAttachmentParameter(FramebufferTarget.Framebuffer, Attachment, FramebufferAttachmentParameterName.ObjectType, out int Type);
                if(Type == (int)GLEnum.None){ continue; }

                Owner.API.GetFramebufferAttachmentParameter(FramebufferTarget.Framebuffer, Attachment, FramebufferAttachmentParameterName.ObjectName, out int ObjectID);
                if(ObjectID <= 0){ continue; }

                if(Type == (int)GLEnum.Texture){
                    GLTexture2D Texture = GLTexture2D.GetExists(Owner, (uint)ObjectID);
                    __Textures[Attachment] = Texture;
                    if(Attachment == FramebufferAttachment.ColorAttachment0){ Viewport = Texture.Size; }
                }else if(Type == (int)GLEnum.Renderbuffer){
                    __RenderBuffers.Add((uint)ObjectID);
                }
            }

            Owner.Pool.SetView(OldView, true);
        }
    }

    public static GLView GetExists(OpenGL Render, uint TargetID){
        if(Render.Pool.RegistryView.TryGetValue(TargetID, out GLView? Result)){ return Result; }
        if(TargetID != 0 && !Render.API.IsFramebuffer(TargetID)){ throw new ExceptionWL("Указан несуществующий ID!"); }

        Result = new GLView(Render, TargetID);
        Render.Pool.RegistryView[TargetID] = Result;

        return Result;
    }
    
    public override void OnDestroy(){
        Owner.Pool.RegistryView.Remove(ID);
        if(object.Equals(Owner.Pool.GetView(), this)){ Owner.Pool.SetView(null, true); }
        if(ID != 0){ Owner.API.DeleteFramebuffer(ID); }

        foreach(uint RenderBuffer in __RenderBuffers){ Owner.API.DeleteRenderbuffer(RenderBuffer); }
        foreach(Texture Texture in __Textures.Values){ Texture.Destroy(); }
        
        __Textures.Clear();
        __RenderBuffers.Clear();
    }
    
    // ----------------------------------------------------------------------
    
    public Vector2I Viewport{ get; set; }

    public Texture? TextureColor0 => GetTexture(FramebufferAttachment.ColorAttachment0);
    public Texture? TextureDepth  => GetTexture(FramebufferAttachment.DepthAttachment);
    
    public Texture? GetTexture(FramebufferAttachment Attachment) => __Textures.TryGetValue(Attachment, out GLTexture2D? Texture) ? Texture : null;
    
    public Color4B[] Get(){
        Color4B[] Pixels = new Color4B[Viewport.X * Viewport.Y];
        GLView OldView = Owner.Pool.GetView();
        Owner.Pool.SetView(this, true);

        unsafe{
            fixed(void* Ptr = Pixels){
                Owner.API.ReadPixels(0, 0, (uint)Viewport.X, (uint)Viewport.Y, PixelFormat.Rgba, PixelType.UnsignedByte, Ptr);
            }
        }
        
        Owner.Pool.SetView(OldView);
        return Pixels;
    }

    public void SetTexture(GLTexture2D? Texture, FramebufferAttachment Attachment = FramebufferAttachment.ColorAttachment0){
        GLView OldView = Owner.Pool.GetView();
        Owner.Pool.SetView(this, true);

        uint TextureID = Texture?.ID ?? 0;

        unsafe{
            Owner.API.FramebufferTexture2D(
                FramebufferTarget.Framebuffer,
                Attachment,
                TextureTarget.Texture2D,
                TextureID,
                0
            );
        }

        if(Texture != null){
            __Textures[Attachment] = Texture;
        }else{
            __Textures.Remove(Attachment);
        }
        
        Owner.Pool.SetView(OldView);
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