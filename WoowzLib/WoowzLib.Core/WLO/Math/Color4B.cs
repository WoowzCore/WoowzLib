using System.Runtime.InteropServices;

namespace WLO.Math;

[StructLayout(LayoutKind.Explicit)]
public struct Color4B : WLI.Serializable{
    [FieldOffset(0)] public byte R;
    [FieldOffset(1)] public byte G;
    [FieldOffset(2)] public byte B;
    [FieldOffset(3)] public byte A;
    
    [FieldOffset(0)] public uint Value;

    public Color4B(byte R, byte G, byte B, byte A = 255){
        Value = 0;
        this.R = R;
        this.G = G;
        this.B = B;
        this.A = A;
    }

    public Color4B(uint Value){
        R = G = B = A = 0;
        this.Value = Value;
    }

    public Dictionary<string, object> Serialize() => new Dictionary<string, object>(){
        [WL.Serializer.__Type] = typeof(Color4B).FullName!,
        ["Value"] = Value
    };

    public void Deserialize(Dictionary<string, object> Data){
        Value = Convert.ToUInt32(Data["Value"]);
    }

    public static Color4B Transparent => new Color4B(0);
}