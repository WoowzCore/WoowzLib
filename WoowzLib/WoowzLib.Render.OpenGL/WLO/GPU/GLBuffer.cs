using Silk.NET.OpenGL;

namespace WLO.GPU;

public class GLBuffer : WLI.GPU.GLResource, WLI.GPU.Buffer{
    private readonly BufferTargetARB __Target;
    
    public uint Size{ get; private set; }

    public GLBuffer(WLO.Render.Hardware.OpenGL Render, BufferTargetARB Target, uint Size) : base(Render){
        __Target = Target;
        this.Size = Size;
        ID = __Owner.API.GenBuffer();

        WLI.GPU.Buffer? OldBuffer = __Owner.GetCBuffer(__Target);
        
        __Owner.SetCBuffer(__Target, this);
        __Owner.API.BufferData(__Target, Size, in nint.Zero, BufferUsageARB.StaticDraw);
        __Owner.SetCBuffer(__Target, OldBuffer);
    }
    
    public unsafe void Update<T>(T[] Data) where T : unmanaged{
        WLI.GPU.Buffer? OldBuffer = __Owner.GetCBuffer(__Target);
        
        __Owner.SetCBuffer(__Target, this);
        fixed(void* Ptr = Data){
            uint DataSize = (uint)(Data.Length * sizeof(T));
            __Owner.API.BufferSubData(__Target, 0, DataSize, Ptr);
        }
        
        __Owner.SetCBuffer(__Target, OldBuffer);
    }

    public override void OnDestroy() => __Owner.API.DeleteBuffer(ID);
}