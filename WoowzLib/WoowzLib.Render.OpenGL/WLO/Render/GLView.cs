using Silk.NET.OpenGL;
using WLO.GPU;
using WLO.Math;
using WLO.Render.Hardware;
using Texture = WLI.GPU.Texture;

namespace WLO.Render;

public class GLView : WLI.GPU.GLResource, WLI_Render.View{
    private readonly Dictionary<FramebufferAttachment, GLTexture2D> __Textures      = [];
    private readonly List      <uint                              > __RenderBuffers = [];
    public PixelLayout Layout{ get; }

    private GLView(OpenGL Render, Vector2I Size, PixelLayout Layout) : base(Render){
        Viewport    = Size;
        this.Layout = Layout;
        ID = Owner.API.GenFramebuffer();
        
        GLView OldView = Owner.Pool.GetView();
        Owner.Pool.SetView(this, true);

        List<GLEnum> ColorAttachments = [];

        foreach(PixelAttribute Attribute in Layout.Attributes){
            if(Attribute.IsTexture){
                PixelFormat CPUFormat = PixelFormat.Rgba;
                PixelType   CPUType   = PixelType.UnsignedByte;
                
                if(Attribute.Attachment == FramebufferAttachment.DepthAttachment){
                    CPUFormat = PixelFormat.DepthComponent;
                    CPUType   = PixelType.Float;
                }else if(Attribute.Attachment == FramebufferAttachment.StencilAttachment){
                    CPUFormat = PixelFormat.StencilIndex;
                    CPUType   = PixelType.UnsignedByte;
                }else if(Attribute.Attachment == FramebufferAttachment.DepthStencilAttachment){
                    CPUFormat = PixelFormat.DepthStencil;
                    CPUType   = PixelType.UnsignedInt248;
                }
                
                GLTexture2D Texture = GLTexture2D.Create(Owner, Size, Attribute.Format, CPUFormat, CPUType);
                Owner.API.FramebufferTexture2D(FramebufferTarget.Framebuffer, Attribute.Attachment, TextureTarget.Texture2D, Texture.ID, 0);
                __Textures[Attribute.Attachment] = Texture;

                if(Attribute.Attachment >= FramebufferAttachment.ColorAttachment0 && Attribute.Attachment <= FramebufferAttachment.ColorAttachment31){
                    ColorAttachments.Add((GLEnum)Attribute.Attachment);
                }
            }else{
                uint RenderBuffer = Owner.API.GenRenderbuffer();
                Owner.API.BindRenderbuffer(RenderbufferTarget.Renderbuffer, RenderBuffer);
                Owner.API.RenderbufferStorage(RenderbufferTarget.Renderbuffer, Attribute.Format, (uint)Size.X, (uint)Size.Y);
                Owner.API.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, Attribute.Attachment, RenderbufferTarget.Renderbuffer, RenderBuffer);
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
            throw new ExceptionWL("todo, framebuffer error (Фреймбуфер не укомплектован!)");
        }
        
        Owner.Pool.SetView(OldView, true);;
    }
    
    public static GLView Create(OpenGL Render, Vector2I Size, PixelLayout Layout){
        GLView Result = new GLView(Render, Size, Layout);
        Render.Pool.RegistryView[Result.ID] = Result;
        return Result;
    }
    
    private GLView(OpenGL Render, uint TargetID) : base(Render){
        FromID = true;
        ID = TargetID;

        List<PixelAttribute> DiscoveredAttributes = new List<PixelAttribute>();
        
        if(ID == 0){
            DiscoveredAttributes.Add(PixelAttribute.Color("Color", 0));
            DiscoveredAttributes.Add(PixelAttribute.Depth());
            Layout = new PixelLayout(DiscoveredAttributes.ToArray());
            
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

                InternalFormat Format;

                if(Type == (int)GLEnum.Texture){
                    Owner.API.GetInteger(GetPName.TextureBinding2D, out int OldTexture); // todo???? нужно свой OpenGL.Pool использовать?
                    Owner.API.BindTexture(TextureTarget.Texture2D, (uint)ObjectID);

                    Owner.API.GetTextureLevelParameter((uint)TextureTarget.Texture2D, 0, GetTextureParameter.TextureInternalFormat, out int Format__);
                    Format = (InternalFormat)Format__;
                    
                    Owner.API.BindTexture(TextureTarget.Texture2D, (uint)OldTexture);
                    
                    GLTexture2D Texture = GLTexture2D.GetExists(Owner, (uint)ObjectID);
                    __Textures[Attachment] = Texture;
                    if(Attachment == FramebufferAttachment.ColorAttachment0){ Viewport = Texture.Size; }
                    
                    DiscoveredAttributes.Add(new PixelAttribute(Attachment.ToString().Replace("Attachment", ""), 4, Attachment, Format, true));
                }else if(Type == (int)GLEnum.Renderbuffer){
                    Owner.API.GetInteger(GetPName.RenderbufferBinding, out int OldRenderBuffer); // todo, отдельный класс RenderBuffer? что это вообще?
                    Owner.API.BindRenderbuffer(RenderbufferTarget.Renderbuffer, (uint)ObjectID);
                
                    Owner.API.GetRenderbufferParameter(RenderbufferTarget.Renderbuffer, RenderbufferParameterName.InternalFormat, out int Format__);
                    Format = (InternalFormat)Format__;
                
                    Owner.API.BindRenderbuffer(RenderbufferTarget.Renderbuffer, (uint)OldRenderBuffer);
                    __RenderBuffers.Add((uint)ObjectID);
                
                    DiscoveredAttributes.Add(new PixelAttribute(Attachment.ToString().Replace("Attachment", ""), 4, Attachment, Format, false));
                }
            }

            Layout = new PixelLayout(DiscoveredAttributes.ToArray());
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
        
        __Textures     .Clear();
        __RenderBuffers.Clear();
    }
    
    // ----------------------------------------------------------------------
    
    public Vector2I Viewport{ get; set; }

    public Texture? TextureColor0 => GetTexture(FramebufferAttachment.ColorAttachment0);
    public Texture? TextureDepth  => GetTexture(FramebufferAttachment.DepthAttachment);
    
    public Texture? GetTexture(FramebufferAttachment Attachment) => __Textures.TryGetValue(Attachment, out GLTexture2D? Texture) ? Texture : null;
    public Texture? GetTexture(string Name){
        PixelAttribute Attribute = Layout.Attributes.FirstOrDefault(a => a.Name == Name);
        if(Attribute.Name == null){ return null; }
        return GetTexture(Attribute.Attachment);
    }
    
    public Color4B[] Get(FramebufferAttachment Attachment = FramebufferAttachment.ColorAttachment0) => GetRect(new Rect2I(0, 0, Viewport.X, Viewport.Y), Attachment);

    public Color4B[] GetRect(Rect2I Rect, FramebufferAttachment Attachment = FramebufferAttachment.ColorAttachment0){
        if(Rect.W <= 0 || Rect.H <= 0){ return []; }

        Color4B[] Pixels = new Color4B[Rect.W * Rect.H];

        GLView OldView = Owner.Pool.GetView();
        Owner.Pool.SetView(this, true);
        
        Owner.API.ReadBuffer((ReadBufferMode)Attachment);

        unsafe{
            fixed(void* Ptr = Pixels){
                Owner.API.ReadPixels(
                    Rect.X, Rect.Y,
                    (uint)Rect.W, (uint)Rect.H,
                    PixelFormat.Rgba,
                    PixelType.UnsignedByte,
                    Ptr
                );
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
}