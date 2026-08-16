using System.Runtime.InteropServices;
using ImGuiNET;
using Silk.NET.OpenGL;
using WLI_Render;
using WLI.GPU;
using WLO.GPU;
using WLO.Math;
using WLO.Render.Hardware;
using Shader = WLI.GPU.Shader;

namespace WLO.Interface;

public class GLImGUI : WLO.Interface.ImGUI, IDisposable{
    private OpenGL __Owner;
    
    public GLImGUI(OpenGL Render, bool StartImmediately = false){
        __Owner = Render;
        
        if(StartImmediately){ Start(); }
    }
    
    public override void Start(){
        try{
            base.Start(); IsStarted = false;

            // language=GLSL
            GLShader VShader = (GLShader)__Owner.CreateShader(Shader.Type.Vertex  , @"#version 330 core
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
            GLShader FShader = (GLShader)__Owner.CreateShader(Shader.Type.Fragment, @"#version 330 core
                in vec2 vUV;
                in vec4 vColor;
                uniform sampler2D uTex;
                out vec4 fColor;
                void main() {
                    fColor = vColor * texture(uTex, vUV);
                }");
            __Program = (GLProgram)__Owner.CreateProgram(VShader, FShader);

            __Uniform_Projection = __Program.GetUniform("uProj");
            __Uniform_Texture    = __Program.GetUniform("uTex");

            unsafe{
                IO.Fonts.GetTexDataAsRGBA32(out byte* Pixels, out int W, out int H);
                __FontTexture = new GLTexture(__Owner, new Vector2I(W, H));

                byte[] ManagedPixels = new byte[W * H * 4];
                Marshal.Copy((IntPtr)Pixels, ManagedPixels, 0, ManagedPixels.Length);
                //todo, set textre pixels...
                __Owner.CTexture = __FontTexture;
                __Owner.API.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, (uint)W, (uint)H, GLEnum.Rgba, PixelType.UnsignedByte, Pixels);
                
                IO.Fonts.SetTexID((IntPtr)__FontTexture.ID);
            }

            __Vertices = new GLBuffer(__Owner, BufferTargetARB.ArrayBuffer, 1024 * 64);
            __Indexes  = new GLBuffer(__Owner, BufferTargetARB.ElementArrayBuffer, 1024 * 16);

            __Mesh = new GLMesh(__Owner);
            VertexLayout Layout = new VertexLayout(
                new VertexAttribute("aPos", 2, VertexAttribute.AttributeType.Float),
                new VertexAttribute("aUV", 2, VertexAttribute.AttributeType.Float),
                new VertexAttribute("aColor", 4, VertexAttribute.AttributeType.Byte, true)
            );
            __Mesh.AddVertexBuffer(__Vertices, Layout);
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

    private GLProgram __Program;
    private GLTexture __FontTexture;
    private GLMesh    __Mesh;
    private GLBuffer  __Vertices;
    private GLBuffer  __Indexes;

    private int __Uniform_Projection;
    private int __Uniform_Texture;
    
    protected override void OnRender(ImDrawDataPtr DrawData){
        unsafe{
            if(DrawData.NativePtr == null || DrawData.CmdListsCount == 0){ return; }
            
            // todo......
            __Owner.API.Enable(GLEnum.Blend);
            __Owner.API.BlendFunc(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha);
            __Owner.API.Disable(GLEnum.CullFace);
            __Owner.API.Disable(GLEnum.DepthTest);
            __Owner.API.Enable(GLEnum.ScissorTest);
            
            Matrix4F proj = Matrix4F.CreateOrtho(
                DrawData.DisplayPos.X, 
                DrawData.DisplayPos.X + DrawData.DisplaySize.X, 
                DrawData.DisplayPos.Y + DrawData.DisplaySize.Y, 
                DrawData.DisplayPos.Y, 
                -1.0f, 1.0f
            );
            
            __Owner.CProgram = __Program;
            __Program.SetUniformM4F(__Uniform_Projection, proj);
            __Program.SetUniformI(__Uniform_Texture, 0);

            for(int i = 0; i < DrawData.CmdListsCount; i++){
                ImDrawListPtr cmdList = DrawData.CmdLists[i];
                
                UpdateBuffer(__Vertices, cmdList.VtxBuffer.Data, (uint)(cmdList.VtxBuffer.Size * sizeof(ImDrawVert)));
                UpdateBuffer(__Indexes, cmdList.IdxBuffer.Data, (uint)(cmdList.IdxBuffer.Size * sizeof(ushort)));

                for(int j = 0; j < cmdList.CmdBuffer.Size; j++){
                    ImDrawCmdPtr cmd = cmdList.CmdBuffer[j];

                    __Owner.API.Scissor(
                        (int)cmd.ClipRect.X, 
                        (int)(DrawData.DisplaySize.Y - cmd.ClipRect.W), 
                        (uint)(cmd.ClipRect.Z - cmd.ClipRect.X), 
                        (uint)(cmd.ClipRect.W - cmd.ClipRect.Y)
                    );
                    
                    // 😨
                    __Owner.API.ActiveTexture(TextureUnit.Texture0);
                    __Owner.API.BindTexture(TextureTarget.Texture2D, (uint)cmd.TextureId);
                    
                    __Owner.API.BindVertexArray(__Mesh.ID);
                    __Owner.API.DrawElements(PrimitiveType.Triangles, cmd.ElemCount, DrawElementsType.UnsignedShort, (void*)(cmd.IdxOffset * sizeof(ushort)));
                }
            }
            
            // todo.....
            __Owner.API.Disable(GLEnum.ScissorTest);
            __Owner.CMesh = null;
            __Owner.CProgram = null;
        }
    }
    
    // todo, временный метод
    private unsafe void UpdateBuffer(GLBuffer buffer, IntPtr data, uint size) {
        __Owner.SetCBuffer(BufferTargetARB.ArrayBuffer, buffer); // Или соответствующий таргет
        __Owner.API.BufferData(BufferTargetARB.ArrayBuffer, size, (void*)data, BufferUsageARB.StreamDraw);
    }
}