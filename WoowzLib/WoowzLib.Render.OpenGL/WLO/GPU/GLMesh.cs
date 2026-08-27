using Silk.NET.OpenGL;
using WLI_Render;
using WLO.Render.Hardware;

namespace WLO.GPU;

public class GLMesh : WLI.GPU.GLResource, WLI.GPU.Mesh{
    private GLMesh(OpenGL Render) : base(Render){
        ID = Owner.API.GenVertexArray();
        if(ID == 0){ throw new ExceptionWL("todo, failed create glmesh"); }
    }

    public static GLMesh Create(OpenGL Render){
        GLMesh Result = new GLMesh(Render);
        
        Render.Pool.RegistryMesh[Result.ID] = Result;

        return Result;
    }
    
    private GLMesh(OpenGL Render, uint TargetID) : base(Render){
        FromID = true;
        ID = TargetID;
        
        // todo, get __CurrentAttributeIndex
    }

    public static GLMesh GetExists(OpenGL Render, uint TargetID){
        if(TargetID == 0){ return null!; }
        
        if(Render.Pool.RegistryMesh.TryGetValue(TargetID, out GLMesh? Result)){ return Result; }

        if(!Render.API.IsVertexArray(TargetID)){ throw new ExceptionWL("Указан несуществующий ID!"); }

        Result = new GLMesh(Render, TargetID);
        Render.Pool.RegistryMesh[TargetID] = Result;

        return Result;
    }

    public override void OnDestroy(){
        Owner.Pool.RegistryMesh.Remove(ID);
        if(object.Equals(Owner.Pool.GetMesh(), this)){ Owner.Pool.SetMesh(null, true); }
        Owner.API.DeleteVertexArray(ID);
    }
    
    // ----------------------------------------------------------------------
    
    private readonly List<WLI.GPU.Buffer> __Vertices = [];
    public IReadOnlyList<WLI.GPU.Buffer> Vertices => __Vertices;
    public WLI.GPU.Buffer? Indices{ get; private set; }

    private uint __CurrentAttributeIndex = 0;
    
    public uint VertexCount{ get; set; }
    public uint IndexCount{ get; private set; }
    
    public unsafe void AddVertexBuffer(WLI.GPU.Buffer Buffer, WLI.GPU.VertexLayout Layout){
        GLMesh?   OldMesh    = Owner.Pool.GetMesh();
        GLBuffer? OldFBuffer = Owner.Pool.GetFBuffer();
        
        Owner.Pool.SetMesh(this, true);
        Owner.Pool.SetFBuffer((GLBuffer)Buffer, true);

        uint Offset = 0;
        foreach(VertexAttribute Attribute in Layout.Attributes){
            Owner.API.EnableVertexAttribArray(__CurrentAttributeIndex);

            if((Attribute.Type is VertexAttribute.AttributeType.Int or VertexAttribute.AttributeType.UInt) && !Attribute.Normalized){
                Owner.API.VertexAttribIPointer(__CurrentAttributeIndex, Attribute.Count, (VertexAttribIType)MapType(Attribute.Type), Layout.Stride, (void*)Offset);
            }else{
                Owner.API.VertexAttribPointer(__CurrentAttributeIndex, Attribute.Count, MapType(Attribute.Type), Attribute.Normalized, Layout.Stride, (void*)Offset);
            }
            
            Offset += (uint)Attribute.Count * VertexAttribute.GetTypeSize(Attribute.Type);
            __CurrentAttributeIndex++;
        }

        if(VertexCount == 0 && Layout.Stride > 0){
            VertexCount = Buffer.Size / Layout.Stride;
        }
        
        __Vertices.Add(Buffer);

        Owner.Pool.SetFBuffer(OldFBuffer, true);
        Owner.Pool.SetMesh(OldMesh, true);
    }
    
    // TODO, не изменямый, фикс позже
    public void SetIndexBuffer(WLI.GPU.Buffer? Buffer, uint IndexCount = 0){
        GLMesh? OldMesh = Owner.Pool.GetMesh();

        Owner.Pool.SetMesh(this, true);
        Indices = Buffer;
        this.IndexCount = IndexCount;

        Owner.Pool.SetIBuffer(Buffer as GLBuffer, true);
        
        Owner.Pool.SetMesh(OldMesh);
    }
    
    // ----------------------------------------------------------------------

    public static VertexAttribPointerType MapType(VertexAttribute.AttributeType Type) => Type switch{
        VertexAttribute.AttributeType.Float => VertexAttribPointerType.Float,
        VertexAttribute.AttributeType.Int   => VertexAttribPointerType.Int,
        VertexAttribute.AttributeType.UInt  => VertexAttribPointerType.UnsignedInt,
        VertexAttribute.AttributeType.Byte  => VertexAttribPointerType.Byte,
        VertexAttribute.AttributeType.UByte => VertexAttribPointerType.UnsignedByte,
        var _ => throw new ArgumentOutOfRangeException(nameof(Type), Type, null)
    };
}