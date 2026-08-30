using WLO.Render;

namespace WLI.GPU;

public interface Mesh : WLI.GPU.Resource{
    void AddVertexBuffer(WLI.GPU.Buffer Buffer, VertexLayout Layout);
    void SetIndexBuffer(WLI.GPU.Buffer? Buffer, uint IndexCount = 0);
    
    uint VertexCount{ get; }
    uint IndexCount{ get; }
}