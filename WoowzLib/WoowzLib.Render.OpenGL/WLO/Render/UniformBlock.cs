using System.Runtime.InteropServices;
using Silk.NET.OpenGL;
using WLO.GPU;
using WLO.Render.Hardware;

namespace WLO.Render;

public class UniformBlock<T> : WLI.Render.UniformBlock, IDisposable where T : unmanaged{
    public GLBuffer Buffer{ get; }

    public uint ID => Buffer.ID;
    
    public UniformBlock(OpenGL Render, BufferUsageARB Usage = BufferUsageARB.DynamicDraw){
        uint Size = (uint)Marshal.SizeOf<T>();

        uint AlignedSize = (Size + 15) & ~15U;
        
        Buffer = GLBuffer.Create(Render, BufferTargetARB.UniformBuffer, AlignedSize, Usage);
    }

    public void Update(T Data) => Buffer.UpdateSingle(Data);
    
    public void Dispose() => Buffer.Destroy();
}