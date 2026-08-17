using Silk.NET.OpenGL;
using WLO.Math;
using WLO.Render.Hardware;

namespace WLO.GPU;

public class GLTexture2D : WLI.GPU.GLResource, WLI.GPU.Texture{
    public Vector2I Size{ get; }

    public readonly InternalFormat GPUFormat;
    public readonly PixelFormat    CPUFormat;
    public readonly PixelType      Type;
    
    public GLTexture2D(OpenGL Render, Vector2I Size, InternalFormat GPUFormat = InternalFormat.Rgba, PixelFormat CPUFormat = PixelFormat.Rgba, PixelType Type = PixelType.UnsignedByte) : base(Render){
        this.Size = Size;
        this.GPUFormat = GPUFormat;
        this.CPUFormat = CPUFormat;
        this.Type = Type;
        ID = __Owner.API.GenTexture();

        WLI.GPU.Texture? OldTexture = __Owner.CTexture;

        __Owner.CTexture = this;

        unsafe{
            __Owner.API.TexImage2D(TextureTarget.Texture2D, 0, GPUFormat, (uint)Size.W, (uint)Size.H, 0, CPUFormat, Type, null);
        }
        
        __Owner.API.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
        __Owner.API.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
        __Owner.API.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS    , (int)GLEnum.Repeat);
        __Owner.API.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT    , (int)GLEnum.Repeat);
        
        __Owner.CTexture = OldTexture;
    }

    public void SetData(IntPtr Pixels, Rect2I? Rect = null){
        Vector2I Offset = Rect?.Position ?? new Vector2I();
        Vector2I Size   = Rect?.Size ?? this.Size;

        WLI.GPU.Texture? OldTexture = __Owner.CTexture;
        __Owner.CTexture = this;

        unsafe{
            __Owner.API.TexSubImage2D(TextureTarget.Texture2D, 0, Offset.X, Offset.Y, (uint)Size.W, (uint)Size.H, CPUFormat, Type, (void*)Pixels);
        }
        
        __Owner.CTexture = OldTexture;
    }
    
    public void SetData<T>(T[] Pixels, Rect2I? Rect = null) where T : unmanaged{
        unsafe{
            fixed(void* Ptr = Pixels){
                SetData((IntPtr)Ptr, Rect);
            }
        }
    }

    public override void OnDestroy() => __Owner.API.DeleteTexture(ID);
}