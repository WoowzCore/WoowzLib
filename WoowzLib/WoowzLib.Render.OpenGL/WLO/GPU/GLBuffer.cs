using Silk.NET.OpenGL;

namespace WLO.GPU;

public class GLBuffer : WLI.GPU.GLResource, WLI.GPU.Buffer{
    private BufferTargetARB __Target;
    public uint Size{ get; private set; }

    public GLBuffer(WLO.Render.Hardware.OpenGL Render, BufferTargetARB Target, uint Size) : base(Render){
        __Target = Target;
        this.Size = Size;
        ID = __Owner.API.GenBuffer();
        __Owner.API.BindBuffer(__Target, ID);
        __Owner.API.BufferData(__Target, Size, nint.Zero, BufferUsageARB.StaticDraw);
    }
    
    public unsafe void Update<T>(T[] Data) where T : unmanaged{
        __Owner.API.BindBuffer(__Target, ID);
        fixed(void* Ptr = Data){
            uint DataSize = (uint)(Data.Length * sizeof(T));
            __Owner.API.BufferSubData(__Target, 0, DataSize, Ptr);
        }
    }

    public override void Dispose() => __Owner.API.DeleteBuffer(ID);
}