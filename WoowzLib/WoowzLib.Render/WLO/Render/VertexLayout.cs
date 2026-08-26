using WLI_Render;

namespace WLI.GPU;

public class VertexLayout{
    public VertexAttribute[] Attributes{ get; }
    public uint Stride{ get; }

    public VertexLayout(params VertexAttribute[] Attributes){
        this.Attributes = Attributes;

        uint Stride__ = 0;
        foreach(VertexAttribute Attribute in Attributes){
            Stride__ += (uint)Attribute.Count * VertexAttribute.GetTypeSize(Attribute.Type);
        }
        Stride = Stride__;
    }
}