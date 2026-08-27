using System.Runtime.InteropServices;
using ImGuiNET;
using Silk.NET.OpenGL;
using WLI_Render;
using WLI.GPU;
using WLO.GPU;
using WLO.Math;
using WLO.Render;
using WLO.Render.Hardware;
using Shader = WLI.GPU.Shader;

namespace WLO.Interface;

public class GLImGUI : WLO.Interface.ImGUI, IDisposable{
    private readonly OpenGL __Owner;
    
    public GLImGUI(OpenGL Render, bool StartImmediately = false){
        __Owner = Render;
        
        if(StartImmediately){ Start(); }
    }
    
    public override void Start(){
        try{
            base.Start(); IsStarted = false;

            // language=GLSL
            GLShader VShader = __Owner.CreateShader(Shader.Type.Vertex  , @"
#version 430 core
layout (location = 0) in vec2 aPos;
layout (location = 1) in vec2 aUV;
layout (location = 2) in vec4 aColor;

uniform mat4 uProj;

out vec2 vUV;
out vec4 vColor;
void main() {
    vUV = aUV;
    vColor = aColor;
    gl_Position = uProj * vec4(aPos, 0, 1);
}");
            
            // language=GLSL
            GLShader FShader = __Owner.CreateShader(Shader.Type.Fragment, @"
#version 430 core
in vec2 vUV;
in vec4 vColor;

uniform sampler2D uTex;

out vec4 fColor;
void main() {
    fColor = vColor * texture(uTex, vUV);
}");
            __Program = __Owner.CreateProgram(VShader, FShader);

            __Uniform_Projection = __Program.GetLocationFromName("uProj") ?? -1;
            __Uniform_Texture    = __Program.GetLocationFromName("uTex")  ?? -1;

            unsafe{
                IO.Fonts.GetTexDataAsRGBA32(out byte* Pixels, out int W, out int H);
                __FontTexture = GLTexture2D.Create(__Owner, new Vector2I(W, H));

                byte[] ManagedPixels = new byte[W * H * 4];
                Marshal.Copy((IntPtr)Pixels, ManagedPixels, 0, ManagedPixels.Length);
                __FontTexture.SetData((IntPtr)Pixels);
                
                IO.Fonts.SetTexID((IntPtr)__FontTexture.ID);
            }

            __Vertices = GLBuffer.Create(__Owner, BufferTargetARB.ArrayBuffer       , 1024 * 512);
            __Indexes  = GLBuffer.Create(__Owner, BufferTargetARB.ElementArrayBuffer, 1024 * 128);

            __Mesh = GLMesh.Create(__Owner);
            __Mesh.AddVertexBuffer(__Vertices, new VertexLayout(
                new VertexAttribute("aPos", 2, VertexAttribute.AttributeType.Float),
                new VertexAttribute("aUV", 2, VertexAttribute.AttributeType.Float),
                new VertexAttribute("aColor", 4, VertexAttribute.AttributeType.UByte, true)
            ));
            __Mesh.SetIndexBuffer(__Indexes);
            
            IsStarted = true;
        }catch(Exception e){
            throw new ExceptionWL("Произошла ошибка при создании ImGUI!", e);
        }
    }
    
    public void Dispose(){
        __Program?.Destroy();
        __FontTexture?.Destroy();
        __Mesh?.Destroy();
    }
    
    // ----------------------------------------------------------------------

    private GLProgram   __Program     = null!;
    private GLTexture2D __FontTexture = null!;
    private GLMesh      __Mesh        = null!;
    private GLBuffer    __Vertices    = null!;
    private GLBuffer    __Indexes     = null!;

    private int __Uniform_Projection;
    private int __Uniform_Texture;
    
    protected override void OnRender(ImDrawDataPtr DrawData){
        unsafe{
            if(DrawData.NativePtr == null || DrawData.CmdListsCount == 0){ return; }

            GLProgram? OldProgram = __Owner.Pool.GetProgram();
            GLMesh?    OldMesh    = __Owner.Pool.GetMesh();
            
            bool OldCullFace    = __Owner.Pool.GetCullFace();
            bool OldDepthTest   = __Owner.Pool.GetDepthTest();
            bool OldScissorTest = __Owner.Pool.GetScissorTest();
            (BlendingFactor, BlendingFactor)? OldBlend = __Owner.Pool.GetBlend();
            
            __Owner.Pool.SetBlend((BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha));
            __Owner.Pool.SetCullFace   (false);
            __Owner.Pool.SetDepthTest  (false);
            __Owner.Pool.SetScissorTest(true );
;            
            Matrix4F Projection = Matrix4F.CreateOrtho(
                DrawData.DisplayPos.X, 
                DrawData.DisplayPos.X + DrawData.DisplaySize.X, 
                DrawData.DisplayPos.Y + DrawData.DisplaySize.Y, 
                DrawData.DisplayPos.Y, 
                -1, 1
            );
            
            __Owner.Pool.SetProgram(__Program);
            
            __Program.SetUniform(UniformValue.CreateM4F(__Uniform_Projection, Projection));
            __Program.SetUniform(UniformValue.CreateI(__Uniform_Texture, 0));
            
            for(int i = 0; i < DrawData.CmdListsCount; i++){
                ImDrawListPtr CMDList = DrawData.CmdLists[i];
                
                    //__Owner.Pool.SetMesh(null, true);
                __Vertices.Update(CMDList.VtxBuffer.Data, (uint)(CMDList.VtxBuffer.Size * sizeof(ImDrawVert)));
                __Indexes .Update(CMDList.IdxBuffer.Data, (uint)(CMDList.IdxBuffer.Size * sizeof(ushort)));
                
                for(int j = 0; j < CMDList.CmdBuffer.Size; j++){
                    ImDrawCmdPtr CMD = CMDList.CmdBuffer[j];

                    __Owner.API.Scissor(
                        (int)CMD.ClipRect.X, 
                        (int)(DrawData.DisplaySize.Y - CMD.ClipRect.W), 
                        (uint)(CMD.ClipRect.Z - CMD.ClipRect.X), 
                        (uint)(CMD.ClipRect.W - CMD.ClipRect.Y)
                    );
                    
                    __Owner.Pool.SetTexture2D(GLTexture2D.GetExists(__Owner, (uint)CMD.TextureId));
                    
                    __Owner.Pool.SetMesh(__Mesh);

                    if(!__Owner.Pool.CanDraw){ continue; }
                    __Owner.Pool.BindForDraw();
                    __Owner.API.DrawElementsBaseVertex(
                        PrimitiveType.Triangles,
                        CMD.ElemCount,
                        DrawElementsType.UnsignedShort,
                        (void*)(CMD.IdxOffset * sizeof(ushort)),
                        (int)CMD.VtxOffset
                    );
                }
            }
            
            __Owner.Pool.SetBlend      (OldBlend      );
            __Owner.Pool.SetCullFace   (OldCullFace   );
            __Owner.Pool.SetDepthTest  (OldDepthTest  );
            __Owner.Pool.SetScissorTest(OldScissorTest);
            
            __Owner.Pool.SetProgram(OldProgram);
            __Owner.Pool.SetMesh(OldMesh);
        }
    }
}