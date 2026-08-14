using System.Runtime.InteropServices;

namespace WLO.Math;

[StructLayout(LayoutKind.Explicit)]
public readonly struct Color4B{
    [FieldOffset(0)] public readonly byte R;
    [FieldOffset(1)] public readonly byte G;
    [FieldOffset(2)] public readonly byte B;
    [FieldOffset(3)] public readonly byte A;
    
    [FieldOffset(0)] public readonly uint Value;

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

    public static Color4B Transparent => new Color4B(0);
}