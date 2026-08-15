using Silk.NET.OpenGL;
using WLI_Render;
using WLO.Render.Hardware;
using Buffer = WLI.GPU.Buffer;

namespace WLO.GPU;

public class GLMesh : GLResource, WLI.GPU.Mesh{
    public Buffer VertexBuffer{ get; }
    public Buffer IndexBuffer{ get; }
    
    public uint VertexCount{ get; }
    public uint IndexCount{ get; private set; }

    public unsafe GLMesh(OpenGL Render, GLBuffer VBO, GLBuffer EBO, uint VCount, uint ICount) : base(Render){
        VertexBuffer = VBO;
        IndexBuffer = EBO;

        VertexCount = VCount;
        IndexCount = ICount;

        ID = __Owner.API.GenVertexArray();
        __Owner.API.BindVertexArray(ID);
        
        __Owner.API.BindBuffer(BufferTargetARB.ArrayBuffer, VertexBuffer.ID);
        if(IndexBuffer != null!){ __Owner.API.BindBuffer(BufferTargetARB.ElementArrayBuffer, IndexBuffer.ID); }
        
        __Owner.API.EnableVertexAttribArray(0);
        __Owner.API.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 12, (void*)0);
        
        __Owner.API.EnableVertexAttribArray(1);
        __Owner.API.VertexAttribPointer(1, 4, VertexAttribPointerType.UnsignedByte, true, 12, (void*)8);
        
        __Owner.API.BindVertexArray(0);
    }
    
    public void Draw(RenderContext Context){
        Context.CurrentMesh = this;
        if(IndexCount > 0){
            Context.DrawIndexed(IndexCount);
        }else{
            Context.Draw(VertexCount);   
        }
    }
    
    public override void Dispose(){
        VertexBuffer.Dispose();
        IndexBuffer?.Dispose();
        __Owner.API.DeleteVertexArray(ID);
    }
}