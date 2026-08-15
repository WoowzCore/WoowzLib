using WLI.GPU;

namespace WLI.GPU;

public interface Mesh : WLI.GPU.Resource{
    WLI.GPU.Buffer VertexBuffer{ get; }
    WLI.GPU.Buffer IndexBuffer { get; }
    
    uint VertexCount{ get; }
    uint IndexCount{ get; }
    
    void Draw(WLI_Render.RenderContext Context);
}