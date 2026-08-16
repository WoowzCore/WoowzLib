using Silk.NET.OpenGL;
using WLO.Math;
using WLO.Render.Hardware;

namespace WLO.GPU;

public class GLTexture : WLI.GPU.GLResource, WLI.GPU.Texture{
    public Vector2I Size{ get; }

    public GLTexture(OpenGL Render, Vector2I Size) : base(Render){
        this.Size = Size;
        ID = __Owner.API.GenTexture();

        WLI.GPU.Texture? OldTexture = __Owner.CTexture;

        __Owner.CTexture = this;

        unsafe{
            __Owner.API.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)Size.W, (uint)Size.H, 0, PixelFormat.Rgba, PixelType.UnsignedByte, null);
        }
        
        __Owner.API.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
        __Owner.API.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
        
        __Owner.CTexture = OldTexture;
    }
    
    public void SetData<T>(T[] Pixels) where T : unmanaged{
        throw new NotImplementedException();
    }

    public override void OnDestroy() => __Owner.API.DeleteTexture(ID);
}