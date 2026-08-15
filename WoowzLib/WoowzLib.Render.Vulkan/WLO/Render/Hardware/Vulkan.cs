using WLI_Render;
using WLI.GPU;
using WLO.Math;
using Buffer = WLI.GPU.Buffer;

namespace WLO.Render.Hardware;

public class Vulkan : WLI_Render.Hardware{
    public void Start(){
        throw new NotImplementedException();
    }
    public void Stop(){
        throw new NotImplementedException();
    }
    public Buffer CreateBuffer(uint Usage, uint Size){
        throw new NotImplementedException();
    }
    public Texture CreateTexture(Vector2I Size, uint Format){
        throw new NotImplementedException();
    }
    public Shader CreateShader(string VertexSource, string FragmentSource){
        throw new NotImplementedException();
    }
    public Mesh CreateMesh<T>(T[] Vertices, uint[]? Indices = null) where T : unmanaged{
        throw new NotImplementedException();
    }
    public RenderView CurrentRenderView{ get; }
    public void FrameStart(RenderView? Target = null){
        throw new NotImplementedException();
    }
    public void FrameStop(){
        throw new NotImplementedException();
    }
}