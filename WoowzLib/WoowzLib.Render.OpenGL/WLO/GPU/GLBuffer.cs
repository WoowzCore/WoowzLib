using Silk.NET.OpenGL;

namespace WLO.GPU;

public class GLBuffer : WLI.GPU.GLResource, WLI.GPU.Buffer{
    private GLBuffer(WLO.Render.Hardware.OpenGL Render, BufferTargetARB Target, uint Size, BufferUsageARB Usage) : base(Render){
        this.Target = Target;
        this.Size = Size;
        this.Usage = Usage;
        ID = Owner.API.GenBuffer();
        if(ID == 0){ throw new ExceptionWL("todo, failed create glbuffer"); }

        GLBuffer? OldBuffer = Owner.Pool.GetBuffer(this.Target);
        
        Owner.Pool.SetBuffer(this.Target, this, true);
        unsafe{ Owner.API.BufferData(this.Target, Size, null, Usage); }
        Owner.Pool.SetBuffer(this.Target, OldBuffer, true);
    }
    
    public static GLBuffer Create(WLO.Render.Hardware.OpenGL Render, BufferTargetARB Target, uint Size, BufferUsageARB Usage = BufferUsageARB.StaticDraw){
        GLBuffer Result = new GLBuffer(Render, Target, Size, Usage);
        
        Render.Pool.RegistryBuffer[Result.ID] = Result;

        return Result;
    }

    private GLBuffer(WLO.Render.Hardware.OpenGL Render, uint TargetID, BufferTargetARB Target) : base(Render){
        FromID = true;
        ID = TargetID;
        this.Target = Target;

        BufferTargetARB QueryTarget = BufferTargetARB.CopyReadBuffer;

        Render.API.GetInteger(GLEnum.CopyReadBufferBinding, out int OldBinding);
        
        Render.API.BindBuffer(QueryTarget, ID);

        Render.API.GetBufferParameter(QueryTarget, GLEnum.BufferSize, out int Size);
        this.Size = (uint)Size;

        Render.API.GetBufferParameter(QueryTarget, GLEnum.BufferUsage, out int Usage);
        this.Usage = (BufferUsageARB)Usage;
        
        Render.API.BindBuffer(QueryTarget, (uint)OldBinding);
    }
    
    public static GLBuffer GetExisting(WLO.Render.Hardware.OpenGL Render, uint TargetID, BufferTargetARB Target = BufferTargetARB.ArrayBuffer){
        if(TargetID == 0){ return null!; }
        
        if(Render.Pool.RegistryBuffer.TryGetValue(TargetID, out GLBuffer? Result)){ return Result; }

        if(!Render.API.IsBuffer(TargetID)){ throw new ExceptionWL("Указан несуществующий ID!"); }

        Result = new GLBuffer(Render, TargetID, Target);
        Render.Pool.RegistryBuffer[TargetID] = Result;

        return Result;
    }
    
    public override void OnDestroy(){
        Owner.Pool.RegistryBuffer.Remove(ID);
        if(object.Equals(Owner.Pool.GetBuffer(Target), this)){ Owner.Pool.SetBuffer(Target, null, true); }
        Owner.API.DeleteBuffer(ID);
    }
    
    // ----------------------------------------------------------------------

    public readonly BufferTargetARB Target;
    public readonly BufferUsageARB  Usage;
    
    public uint Size{ get; }
    
    public void Update(IntPtr Data, uint DataSize, uint Offset = 0){
        if(Data == IntPtr.Zero){ throw new ExceptionWL("todo"); }
        
        if(Offset + DataSize > Size){ throw new ExceptionWL("todo"); }
        
        GLBuffer? OldBuffer = Owner.Pool.GetBuffer(Target);
        Owner.Pool.SetBuffer(Target, this, true);
        
        unsafe{ Owner.API.BufferSubData(Target, (nint)Offset, DataSize, (void*)Data); }
        
        Owner.Pool.SetBuffer(Target, OldBuffer, true);
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
            
            GLBuffer? OldBuffer = Owner.Pool.GetBuffer(Target);
            Owner.Pool.SetBuffer(Target, this, true);

            fixed(void* Ptr = Destination){
                Owner.API.GetBufferSubData(Target, (nint)Offset, ReadSize, Ptr);
            }
            
            Owner.Pool.SetBuffer(Target, OldBuffer, true);
        }
    }
}