using Silk.NET.OpenGL;
using WLI_Render;
using WLO.Render.Hardware;

namespace WLO.GPU;

public class GLMesh : WLI.GPU.GLResource, WLI.GPU.Mesh{
    private readonly List<WLI.GPU.Buffer> __VBO = [];
    private               WLI.GPU.Buffer? __EBO;

    private uint __CurrentAttributeIndex = 0;
    
    public uint VertexCount{ get; set; }
    public uint IndexCount{ get; private set; }

    // ----------------------------------------------------------------------
    
    public GLMesh(OpenGL Render) : base(Render){
        ID = __Owner.API.GenVertexArray();
    }
    
    public unsafe void AddVertexBuffer(WLI.GPU.Buffer Buffer, WLI.GPU.VertexLayout Layout){
        WLI.GPU.Mesh? OldMesh = __Owner.Context.CMesh;
        WLI.GPU.Buffer? OldFBuffer = __Owner.Context.CurrentFloatBuffer;
        
        __Owner.Context.CMesh        = this;
        __Owner.Context.CurrentFloatBuffer = Buffer;

        uint Offset = 0;
        foreach(VertexAttribute Attribute in Layout.Attributes){
            __Owner.API.EnableVertexAttribArray(__CurrentAttributeIndex);
            __Owner.API.VertexAttribPointer(__CurrentAttributeIndex, Attribute.Count, MapType(Attribute.Type), Attribute.Normalized, Layout.Stride, (void*)Offset);
            Offset += (uint)Attribute.Count * VertexAttribute.GetTypeSize(Attribute.Type);
            __CurrentAttributeIndex++;
        }

        if(VertexCount == 0 && Layout.Stride > 0){
            VertexCount = Buffer.Size / Layout.Stride;
        }
        
        __VBO.Add(Buffer);

        __Owner.Context.CurrentFloatBuffer = OldFBuffer;
        __Owner.Context.CMesh = OldMesh;
    }
    
    public void SetIndexBuffer(WLI.GPU.Buffer? Buffer, uint IndexCount = 0){
        WLI.GPU.Mesh? OldMesh = __Owner.Context.CMesh;

        __Owner.Context.CMesh = this;
        __EBO = Buffer;
        this.IndexCount = IndexCount;

        __Owner.API.BindBuffer(BufferTargetARB.ElementArrayBuffer, Buffer?.ID ?? 0);

        __Owner.Context.CMesh = OldMesh;
    }
    
    // ----------------------------------------------------------------------
    
    public void Draw(RenderContext Context){
        Context.CMesh = this;
        if(IndexCount > 0){
            Context.DrawIndexed(IndexCount);
        }else{
            Context.Draw(VertexCount);   
        }
    }
    
    // ----------------------------------------------------------------------
    
    public override void Dispose() => __Owner.API.DeleteVertexArray(ID);

    public static VertexAttribPointerType MapType(VertexAttribute.AttributeType Type) => Type switch{
        VertexAttribute.AttributeType.Float => VertexAttribPointerType.Float,
        VertexAttribute.AttributeType.Int => VertexAttribPointerType.Int,
        VertexAttribute.AttributeType.Byte => VertexAttribPointerType.UnsignedByte,
        var _ => throw new ArgumentOutOfRangeException(nameof(Type), Type, null)
    };
}