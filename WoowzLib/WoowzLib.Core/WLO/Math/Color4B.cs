using System.Globalization;
using System.Runtime.InteropServices;

namespace WLO.Math;

[StructLayout(LayoutKind.Explicit)]
public struct Color4B : WLI.Packable{
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

    public Dictionary<string, object?> __Pack() => new Dictionary<string, object?>{
        ["RGBA"] = $"{R}|{G}|{B}|{A}"
    };

    public void __Unpack(Dictionary<string, object?> Data){
        string RGBA = WL.Packer.Get<string>(Data, "RGBA", "0|0|0|0")!;

        string[] Parts = RGBA.Split("|");
        if(Parts.Length >= 3){
            byte.TryParse(Parts[0], out R);
            byte.TryParse(Parts[1], out G);
            byte.TryParse(Parts[2], out B);
            byte.TryParse(Parts[3], out A);
        }
    }

    public static Color4B Transparent => new Color4B(0, 0, 0, 0);
    public static Color4B White => new Color4B(255, 255, 255);
    public static Color4B Black => new Color4B(0, 0, 0);
}