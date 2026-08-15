using WLI.GPU;

namespace WLI.GPU;

public interface Mesh : WLI.GPU.Resource{
    WLI.GPU.Buffer VertexBuffer{ get; }
    WLI.GPU.Buffer IndexBuffer { get; }
    
    uint VertexCount{ get; }
    void Draw(WLI_Render.RenderContext Context);
}