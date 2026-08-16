using Silk.NET.OpenGL;
using WLI.GPU;
using WLO.Math;
using WLO.Render.Hardware;
using Buffer = WLI.GPU.Buffer;
using Program = WLI.GPU.Program;

namespace WLO.Render;

public class GLRenderContext : WLI_Render.RenderContext{
    private readonly OpenGL __Owner;

    public GLRenderContext(OpenGL Render) => __Owner = Render;

    private Program? __CurrentProgram = null!;
    public Program? CProgram{
        get => __CurrentProgram;
        set{
            uint OldID = __CurrentProgram?.ID ?? 0;
            uint NewID = value?.ID ?? 0;
            if(OldID == NewID){ return; }
            __Owner.API.UseProgram(NewID);
            __CurrentProgram = value;
        }
    }
    
    private Mesh? __CurrentMesh = null!;
    public Mesh? CMesh{
        get => __CurrentMesh;
        set{
            uint OldID = __CurrentMesh?.ID ?? 0;
            uint NewID = value?.ID ?? 0;
            if(OldID == NewID){ return; }
            __Owner.API.BindVertexArray(NewID);
            __CurrentMesh = value;
        }
    }
    
    private Buffer? __CurrentFloatBuffer = null!;
    public Buffer? CurrentFloatBuffer{
        get => __CurrentFloatBuffer;
        set{
            uint OldID = __CurrentFloatBuffer?.ID ?? 0;
            uint NewID = value?.ID ?? 0;
            if(OldID == NewID){ return; }
            __Owner.API.BindBuffer(BufferTargetARB.ArrayBuffer, NewID);
            __CurrentFloatBuffer = value;
        }
    }
    
    // ----------------------------------------------------------------------
    
    public void Clear(Color4B Color){
        __Owner.API.ClearColor(Color.R / 255f, Color.G / 255f, Color.B / 255f, Color.A / 255f);
        __Owner.API.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
    }
    
    public void Draw(uint Count, uint Start = 0){
        if(CProgram == null! || CMesh == null!){ return; }

        __Owner.API.UseProgram(CProgram.ID);
        __Owner.API.BindVertexArray(CMesh.ID);
        __Owner.API.DrawArrays(PrimitiveType.Triangles, (int)Start, Count);
        __Owner.API.BindVertexArray(0);
    }
    
    public unsafe void DrawIndexed(uint Count, uint StartIndex = 0, int BaseVertex = 0){
        if(CProgram == null! || CMesh == null!){ return; }
        
        __Owner.API.UseProgram(CProgram.ID);
        __Owner.API.BindVertexArray(CMesh.ID);
        
        void* Offset = (void*)(StartIndex * sizeof(uint));
        if(BaseVertex == 0){
            __Owner.API.DrawElements(PrimitiveType.Triangles, Count, DrawElementsType.UnsignedInt, Offset);   
        }else{
            __Owner.API.DrawElementsBaseVertex(PrimitiveType.Triangles, Count, DrawElementsType.UnsignedInt, Offset, BaseVertex);  
        }
        
        __Owner.API.BindVertexArray(0);
    }
}