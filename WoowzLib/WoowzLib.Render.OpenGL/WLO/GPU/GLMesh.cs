using Silk.NET.OpenGL;
using WLI_Render;
using WLO.Render.Hardware;

namespace WLO.GPU;

public class GLMesh : WLI.GPU.GLResource, WLI.GPU.Mesh{
    private GLMesh(OpenGL Render) : base(Render){
        ID = __Owner.API.GenVertexArray();
    }

    public static GLMesh Create(OpenGL Render){
        GLMesh Result = new GLMesh(Render);
        
        Render.Registry_Mesh[Result.ID] = Result;

        return Result;
    }
    
    private GLMesh(OpenGL Render, uint TargetID) : base(Render){
        FromID = true;
        ID = TargetID;
        
        // todo, get __CurrentAttributeIndex
    }

    public static GLMesh GetExists(OpenGL Render, uint TargetID){
        if(TargetID == 0){ return null!; }
        
        if(Render.Registry_Mesh.TryGetValue(TargetID, out GLMesh Result)){ return Result; }

        if(!Render.API.IsVertexArray(TargetID)){ throw new ExceptionWL("Указан несуществующий ID!"); }

        Result = new GLMesh(Render, TargetID);
        Render.Registry_Mesh[TargetID] = Result;

        return Result;
    }

    public override void OnDestroy(){
        __Owner.Registry_Mesh.Remove(ID);
        __Owner.API.DeleteVertexArray(ID);
    }
    
    // ----------------------------------------------------------------------
    
    private readonly List<WLI.GPU.Buffer> __VBO = [];
    private          WLI.GPU.Buffer?      __EBO;

    private uint __CurrentAttributeIndex = 0;
    
    public uint VertexCount{ get; set; }
    public uint IndexCount{ get; private set; }
    
    public unsafe void AddVertexBuffer(WLI.GPU.Buffer Buffer, WLI.GPU.VertexLayout Layout){
        WLI.GPU.Mesh?   OldMesh    = __Owner.CMesh;
        WLI.GPU.Buffer? OldFBuffer = __Owner.CFBuffer;
        
        __Owner.CMesh    = this;
        __Owner.CFBuffer = Buffer;

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

        __Owner.CFBuffer = OldFBuffer;
        __Owner.CMesh    = OldMesh;
    }
    
    // TODO, не изменямый, фикс позже
    public void SetIndexBuffer(WLI.GPU.Buffer? Buffer, uint IndexCount = 0){
        WLI.GPU.Mesh? OldMesh = __Owner.CMesh;

        __Owner.CMesh = this;
        __EBO = Buffer;
        this.IndexCount = IndexCount;

        __Owner.API.BindBuffer(BufferTargetARB.ElementArrayBuffer, Buffer?.ID ?? 0);

        __Owner.CMesh = OldMesh;
    }
    
    // ----------------------------------------------------------------------

    public static VertexAttribPointerType MapType(VertexAttribute.AttributeType Type) => Type switch{
        VertexAttribute.AttributeType.Float => VertexAttribPointerType.Float,
        VertexAttribute.AttributeType.Int => VertexAttribPointerType.Int,
        VertexAttribute.AttributeType.Byte => VertexAttribPointerType.UnsignedByte,
        var _ => throw new ArgumentOutOfRangeException(nameof(Type), Type, null)
    };
}