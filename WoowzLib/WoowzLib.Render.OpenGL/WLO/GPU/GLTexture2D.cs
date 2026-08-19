using Silk.NET.OpenGL;
using WLO.Math;
using WLO.Render.Hardware;
using Texture = WLI.GPU.Texture;

namespace WLO.GPU;

public class GLTexture2D : WLI.GPU.GLResource, WLI.GPU.Texture{
    private GLTexture2D(OpenGL Render, Vector2I Size, InternalFormat GPUFormat, PixelFormat CPUFormat, PixelType Type) : base(Render){
        this.Size = Size;
        this.GPUFormat = GPUFormat;
        this.CPUFormat = CPUFormat;
        this.Type = Type;
        ID = __Owner.API.GenTexture();

        WLI.GPU.Texture? OldTexture = __Owner.CTexture2D;

        __Owner.CTexture2D = this;

        unsafe{
            __Owner.API.TexImage2D(TextureTarget.Texture2D, 0, GPUFormat, (uint)Size.W, (uint)Size.H, 0, CPUFormat, Type, null);
        }
        
        __Owner.API.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
        __Owner.API.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
        __Owner.API.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS    , (int)GLEnum.Repeat);
        __Owner.API.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT    , (int)GLEnum.Repeat);
        
        __Owner.CTexture2D = OldTexture;
    }
    
    public static GLTexture2D Create(OpenGL Render, Vector2I Size, InternalFormat GPUFormat = InternalFormat.Rgba, PixelFormat CPUFormat = PixelFormat.Rgba, PixelType Type = PixelType.UnsignedByte){
        GLTexture2D Result = new GLTexture2D(Render, Size, GPUFormat, CPUFormat, Type);
        
        Render.Registry_Texture2D[Result.ID] = Result;

        return Result;
    }
    
    private GLTexture2D(OpenGL Render, uint TargetID) : base(Render){
        FromID = true;
        ID = TargetID;

        WLI.GPU.Texture Old = __Owner.CTexture2D;

        __Owner.API.GetTexLevelParameter(TextureTarget.Texture2D, 0, GetTextureParameter.TextureWidth , out int W);
        __Owner.API.GetTexLevelParameter(TextureTarget.Texture2D, 0, GetTextureParameter.TextureHeight, out int H);
        Size = new Vector2I(W, H);
        
        __Owner.API.GetTexLevelParameter(TextureTarget.Texture2D, 0, GetTextureParameter.TextureInternalFormat, out int InternalFormat);
        GPUFormat = (InternalFormat)InternalFormat;
        
        //todo получать другие форматы (ЧЕРЕЗ ДОГАДКИ.....)
        
        __Owner.CTexture2D = Old;
    }

    public static GLTexture2D GetExists(OpenGL Render, uint TargetID){
        if(TargetID == 0){ return null!; }
        
        if(Render.Registry_Texture2D.TryGetValue(TargetID, out GLTexture2D Result)){ return Result; }

        if(!Render.API.IsTexture(TargetID)){ throw new ExceptionWL("Указан несуществующий ID!"); }

        Result = new GLTexture2D(Render, TargetID);
        Render.Registry_Texture2D[TargetID] = Result;

        return Result;
    }

    public override void OnDestroy(){
        __Owner.Registry_Texture2D.Remove(ID);
        __Owner.API.DeleteTexture(ID);
    }
    
    // ----------------------------------------------------------------------
    
    public Vector2I Size{ get; }

    public readonly InternalFormat GPUFormat;
    public readonly PixelFormat    CPUFormat;
    public readonly PixelType      Type;
    
    public void SetData(IntPtr Pixels, Rect2I? Rect = null){
        Vector2I Offset = Rect?.Position ?? new Vector2I();
        Vector2I Size   = Rect?.Size ?? this.Size;

        WLI.GPU.Texture? OldTexture = __Owner.CTexture2D;
        __Owner.CTexture2D = this;

        unsafe{
            __Owner.API.TexSubImage2D(TextureTarget.Texture2D, 0, Offset.X, Offset.Y, (uint)Size.W, (uint)Size.H, CPUFormat, Type, (void*)Pixels);
        }
        
        __Owner.CTexture2D = OldTexture;
    }
    
    public void SetData<T>(T[] Pixels, Rect2I? Rect = null) where T : unmanaged{
        unsafe{
            fixed(void* Ptr = Pixels){
                SetData((IntPtr)Ptr, Rect);
            }
        }
    }
}