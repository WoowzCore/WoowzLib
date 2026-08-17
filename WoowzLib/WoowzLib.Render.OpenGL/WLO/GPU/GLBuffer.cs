using Silk.NET.OpenGL;

namespace WLO.GPU;

public class GLBuffer : WLI.GPU.GLResource, WLI.GPU.Buffer{
    public readonly BufferTargetARB Target;
    public  readonly BufferUsageARB Usage;
    
    public uint Size{ get; private set; }

    public GLBuffer(WLO.Render.Hardware.OpenGL Render, BufferTargetARB Target, uint Size, BufferUsageARB Usage = BufferUsageARB.StaticDraw) : base(Render){
        this.Target = Target;
        this.Size = Size;
        this.Usage = Usage;
        ID = __Owner.API.GenBuffer();

        WLI.GPU.Buffer? OldBuffer = __Owner.GetCBuffer(this.Target);
        
        __Owner.SetCBuffer(this.Target, this);
        unsafe{ __Owner.API.BufferData(this.Target, Size, null, Usage); }
        __Owner.SetCBuffer(this.Target, OldBuffer);
    }

    public void Update(IntPtr Data, uint DataSize, uint Offset = 0){
        if(Data == IntPtr.Zero){ throw new ExceptionWL("todo"); }

        if(Offset + DataSize > Size){ throw new ExceptionWL("todo"); }
        
        WLI.GPU.Buffer? OldBuffer = __Owner.GetCBuffer(Target);
        __Owner.SetCBuffer(Target, this);

        unsafe{ __Owner.API.BufferSubData(Target, (nint)Offset, DataSize, (void*)Data); }
            
        __Owner.SetCBuffer(Target, OldBuffer);
    }
    
    public void Update<T>(ReadOnlySpan<T> Data, uint Offset = 0) where T : unmanaged{
        unsafe{
            fixed(void* Ptr = Data){
                Update((IntPtr)Ptr, (uint)(Data.Length * sizeof(T)), Offset);
            }
        }
    }

    public void Update<T>(T[] Data, uint Offset = 0) where T : unmanaged => Update(new ReadOnlySpan<T>(Data), Offset);

    public void Read<T>(Span<T> Destination, uint Offset = 0) where T : unmanaged{
        unsafe{
            uint ReadSize = (uint)(Destination.Length * sizeof(T));

            if(Offset + ReadSize > Size){ throw new ExceptionWL("todo"); }
            
            WLI.GPU.Buffer? OldBuffer = __Owner.GetCBuffer(Target);
            __Owner.SetCBuffer(Target, this);

            fixed(void* Ptr = Destination){
                __Owner.API.GetBufferSubData(Target, (nint)Offset, ReadSize, Ptr);
            }
            
            __Owner.SetCBuffer(Target, OldBuffer);
        }
    }

    public override void OnDestroy() => __Owner.API.DeleteBuffer(ID);
}