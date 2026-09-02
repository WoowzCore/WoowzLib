namespace WLO.Render;

public struct VertexAttribute{
    public string        Name;
    public int           Count;
    public AttributeType Type;
    public bool          Normalized;

    public VertexAttribute(string Name, int Count, AttributeType Type, bool Normalized = false){
        this.Name       = Name;
        this.Count      = Count;
        this.Type       = Type;
        this.Normalized = Normalized;
    }
    
    public enum AttributeType{
        Float,
        Int,
        UInt,
        Byte,
        UByte
    }

    public static uint GetTypeSize(AttributeType Type) => Type switch{
        AttributeType.Float => 4,
        AttributeType.Int   => 4,
        AttributeType.UInt  => 4,
        AttributeType.Byte  => 1, /* -128 = 128 */
        AttributeType.UByte => 1, /* 0 = 255 */
        var _ => throw new ArgumentOutOfRangeException(nameof(Type), Type, null)
    };
}