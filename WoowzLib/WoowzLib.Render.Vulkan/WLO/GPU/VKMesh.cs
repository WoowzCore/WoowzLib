using WLI_Render;
using Buffer = WLI.GPU.Buffer;

namespace WLO.GPU;

public class VKMesh : WLI.GPU.Mesh{
    public void Dispose(){
        throw new NotImplementedException();
    }
    public uint ID{ get; }
    public Buffer VertexBuffer{ get; }
    public Buffer IndexBuffer{ get; }
    public uint VertexCount{ get; }
    public void Draw(RenderContext Context){
        throw new NotImplementedException();
    }
}