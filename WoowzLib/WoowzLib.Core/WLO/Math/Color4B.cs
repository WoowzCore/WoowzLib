using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WLO.Math;

[StructLayout(LayoutKind.Explicit)]
public struct Color4B : IEquatable<Color4B>, WLI.Packable{
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

    // ----------------------------------------------------------------------
    
    // todo, добавить offset и возможность разнообразия тута, сиды короче
    
    private const uint __IDColor_Mask24     = 0xFFFFFFu;
    private const uint __IDColor_Multiplier = 0x5BF037u;
    private const uint __IDColor_Inverse    = 0xA6C587u;
    
    public static Color4B FromUInt(uint UInt){
        if(UInt == 0){ return Color4B.Black; }

        uint X = (UInt * __IDColor_Multiplier) & __IDColor_Mask24;

        byte R = (byte)(X >> 0  & 0xFF);
        byte G = (byte)(X >> 8  & 0xFF);
        byte B = (byte)(X >> 16 & 0xFF);

        return new Color4B(R, G, B, 255);
    }

    public uint ToUInt(){
        if(R == 0 && G == 0 && B == 0){ return 0; }

        uint X = (uint)R << 0 | (uint)G << 8 | (uint)B << 16;

        return (X * __IDColor_Inverse) & __IDColor_Mask24;
    }

    // ----------------------------------------------------------------------

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

    // ----------------------------------------------------------------------
    
    public static Color4B Transparent => new Color4B(0, 0, 0, 0);
    public static Color4B White => new Color4B(255, 255, 255);
    public static Color4B Black => new Color4B(0, 0, 0);
    
    // ----------------------------------------------------------------------
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Color4B Other) => R == Other.R && G == Other.G && B == Other.B && A == Other.A;

    public override bool Equals(object? Object) => Object is Color4B Other && Equals(Other);

    public override int GetHashCode() => HashCode.Combine(R, G, B, A);

    public static bool operator ==(Color4B L, Color4B R) =>  L.Equals(R);
    public static bool operator !=(Color4B L, Color4B R) => !L.Equals(R);

    public override string ToString() => $"Color4B({R}, {G}, {B}, {A})";
}