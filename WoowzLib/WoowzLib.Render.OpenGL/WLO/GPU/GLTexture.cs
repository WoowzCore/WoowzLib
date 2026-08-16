using Silk.NET.OpenGL;
using WLO.Math;
using WLO.Render.Hardware;

namespace WLO.GPU;

public class GLTexture : WLI.GPU.GLResource, WLI.GPU.Texture{
    public Vector2I Size{ get; }

    public GLTexture(OpenGL Render, Vector2I Size) : base(Render){
        this.Size = Size;
        ID = __Owner.API.GenTexture();
        
    }
    
    public void SetData<T>(T[] Pixels) where T : unmanaged{
        throw new NotImplementedException();
    }

    public override void Dispose() => __Owner.API.DeleteTexture(ID);
}