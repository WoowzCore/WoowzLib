using Silk.NET.OpenGL;
using WLI.GPU;
using WLO.GPU;
using WLO.Math;
using WLO.Render.Hardware;
using Program = WLI.GPU.Program;
using Shader = WLI.GPU.Shader;

namespace WLO.Render;

public class GLRenderContext : WLI_Render.RenderContext{
    private readonly OpenGL __Owner;

    public GLRenderContext(OpenGL Render) => __Owner = Render;
    
    public Program CurrentProgram{ get; set; } = null!;
    public Mesh CurrentMesh{ get; set; } = null!;
    
    // ----------------------------------------------------------------------
    
    public void Clear(Color4B Color){
        __Owner.API.ClearColor(Color.R / 255f, Color.G / 255f, Color.B / 255f, Color.A / 255f);
        __Owner.API.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
    }
    
    public void Draw(uint Count, uint Start = 0){
        if(CurrentProgram == null! || CurrentMesh == null!){ return; }

        __Owner.API.UseProgram(CurrentProgram.ID);
        __Owner.API.BindVertexArray(CurrentMesh.ID);
        __Owner.API.DrawArrays(PrimitiveType.Triangles, (int)Start, Count);
        __Owner.API.BindVertexArray(0);
    }
    
    public unsafe void DrawIndexed(uint Count, uint StartIndex = 0, int BaseVertex = 0){
        if(CurrentProgram == null! || CurrentMesh == null!){ return; }
        
        __Owner.API.UseProgram(CurrentProgram.ID);
        __Owner.API.BindVertexArray(CurrentMesh.ID);
        
        void* Offset = (void*)(StartIndex * sizeof(uint));
        if(BaseVertex == 0){
            __Owner.API.DrawElements(PrimitiveType.Triangles, Count, DrawElementsType.UnsignedInt, Offset);   
        }else{
            __Owner.API.DrawElementsBaseVertex(PrimitiveType.Triangles, Count, DrawElementsType.UnsignedInt, Offset, BaseVertex);  
        }
        
        __Owner.API.BindVertexArray(0);
    }
}