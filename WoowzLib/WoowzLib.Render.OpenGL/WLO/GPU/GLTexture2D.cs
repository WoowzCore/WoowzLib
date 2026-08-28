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
        ID = Owner.API.GenTexture();
        if(ID == 0){ throw new ExceptionWL("todo, failed create gltexture2d"); }

        GLTexture2D? OldTexture2D = Owner.Pool.GetTexture2D();

        Owner.Pool.SetTexture2D(this, 0, true);

        unsafe{
            Owner.API.TexImage2D(TextureTarget.Texture2D, 0, GPUFormat, (uint)Size.W, (uint)Size.H, 0, CPUFormat, Type, null);
        }
        
        SetFilter(TextureMinFilter.Linear);
        SetWrap  (TextureWrapMode.Repeat);
        
        Owner.Pool.SetTexture2D(OldTexture2D, 0, true);
    }
    
    public static GLTexture2D Create(OpenGL Render, Vector2I Size, InternalFormat GPUFormat = InternalFormat.Rgba, PixelFormat CPUFormat = PixelFormat.Rgba, PixelType Type = PixelType.UnsignedByte){
        GLTexture2D Result = new GLTexture2D(Render, Size, GPUFormat, CPUFormat, Type);
        
        Render.Pool.RegistryTexture2D[Result.ID] = Result;

        return Result;
    }
    
    private GLTexture2D(OpenGL Render, uint TargetID) : base(Render){
        FromID = true;
        ID = TargetID;

        GLTexture2D? OldTexture2D = Owner.Pool.GetTexture2D();

        Owner.API.GetTexLevelParameter(TextureTarget.Texture2D, 0, GetTextureParameter.TextureWidth , out int W);
        Owner.API.GetTexLevelParameter(TextureTarget.Texture2D, 0, GetTextureParameter.TextureHeight, out int H);
        Size = new Vector2I(W, H);
        
        Owner.API.GetTexLevelParameter(TextureTarget.Texture2D, 0, GetTextureParameter.TextureInternalFormat, out int InternalFormat);
        GPUFormat = (InternalFormat)InternalFormat;
        
        //todo получать другие форматы (ЧЕРЕЗ ДОГАДКИ.....)
        
        Owner.Pool.SetTexture2D(OldTexture2D, 0, true);
    }

    public static GLTexture2D GetExists(OpenGL Render, uint TargetID){
        if(TargetID == 0){ return null!; }
        
        if(Render.Pool.RegistryTexture2D.TryGetValue(TargetID, out GLTexture2D? Result)){ return Result; }

        if(!Render.API.IsTexture(TargetID)){ throw new ExceptionWL("Указан несуществующий ID!"); }

        Result = new GLTexture2D(Render, TargetID);
        Render.Pool.RegistryTexture2D[TargetID] = Result;

        return Result;
    }

    public override void OnDestroy(){
        Owner.Pool.RegistryTexture2D.Remove(ID);
        for(uint i = 0; i < Owner.Pool.MaxTextureSlots; i++){ if(object.Equals(Owner.Pool.GetTexture2D(i), this)){ Owner.Pool.SetTexture2D(null, i, true); } }
        Owner.API.DeleteTexture(ID);
    }
    
    // ----------------------------------------------------------------------
    
    public Vector2I Size{ get; }

    public readonly InternalFormat GPUFormat;
    public readonly PixelFormat    CPUFormat;
    public readonly PixelType      Type;
    
    public void Update(IntPtr Pixels, Rect2I? Rect = null){
        Vector2I Offset = Rect?.Position ?? new Vector2I();
        Vector2I Size   = Rect?.Size ?? this.Size;

        GLTexture2D? OldTexture2D = Owner.Pool.GetTexture2D();
        Owner.Pool.SetTexture2D(this, 0, true);

        unsafe{
            Owner.API.TexSubImage2D(TextureTarget.Texture2D, 0, Offset.X, Offset.Y, (uint)Size.W, (uint)Size.H, CPUFormat, Type, (void*)Pixels);
        }
        
        Owner.Pool.SetTexture2D(OldTexture2D, 0, true);
    }
    
    public void Update<T>(T[] Pixels, Rect2I? Rect = null) where T : unmanaged{
        unsafe{
            fixed(void* Ptr = Pixels){
                Update((IntPtr)Ptr, Rect);
            }
        }
    }

    public void SetFilter(TextureMinFilter Min, TextureMagFilter Mag){
        GLTexture2D? OldTexture2D = Owner.Pool.GetTexture2D();
        Owner.Pool.SetTexture2D(this, 0, true);

        Owner.API.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)Min);
        Owner.API.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)Mag);

        Owner.Pool.SetTexture2D(OldTexture2D, 0, true);
    }

    public void SetFilter(TextureMinFilter MinMag) => SetFilter(MinMag, (TextureMagFilter)MinMag);
    
    public void SetWrap(TextureWrapMode Horizontal, TextureWrapMode Vertical) {
        GLTexture2D? OldTexture2D = Owner.Pool.GetTexture2D();
        Owner.Pool.SetTexture2D(this, 0, true);

        Owner.API.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)Horizontal);
        Owner.API.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)Vertical);

        Owner.Pool.SetTexture2D(OldTexture2D, 0, true);
    }
    
    public void SetWrap(TextureWrapMode HorizontalVertical) => SetWrap(HorizontalVertical, HorizontalVertical);
}