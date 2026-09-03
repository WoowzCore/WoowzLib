using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace WLO.Math;

public struct Vector4F : IEquatable<Vector4F>, WLI.Packable{
    public float X;
    public float Y;
    public float Z;
    public float W;

    public float R => X;
    public float G => Y;
    public float B => Z;
    public float A => W;

    public Vector4F(float X, float Y, float Z, float W){
        this.X = X;
        this.Y = Y;
        this.Z = Z;
        this.W = W;
    }
    public Vector4F(float XYZW) : this(XYZW, XYZW, XYZW, XYZW){}
    public Vector4F(Vector3F XYZ, float W) : this(XYZ.X, XYZ.Y, XYZ.Z, W){}

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Vector128<float> AsVector128() => Vector128.Create(X, Y, Z, W);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4F operator +(Vector4F A, Vector4F B){
        Vector128<float> Result = Vector128.Add(A.AsVector128(), B.AsVector128());
        return new Vector4F(Result.GetElement(0), Result.GetElement(1), Result.GetElement(2), Result.GetElement(3));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4F operator -(Vector4F A, Vector4F B){
        Vector128<float> Result = Vector128.Subtract(A.AsVector128(), B.AsVector128());
        return new Vector4F(Result.GetElement(0), Result.GetElement(1), Result.GetElement(2), Result.GetElement(3));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4F operator *(Vector4F A, Vector4F B){
        Vector128<float> Result = Vector128.Multiply(A.AsVector128(), B.AsVector128());
        return new Vector4F(Result.GetElement(0), Result.GetElement(1), Result.GetElement(2), Result.GetElement(3));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4F operator +(Vector4F A, float B){
        Vector128<float> Result = Vector128.Add(A.AsVector128(), Vector128.Create(B));
        return new Vector4F(Result.GetElement(0), Result.GetElement(1), Result.GetElement(2), Result.GetElement(3));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4F operator *(Vector4F A, float B){
        Vector128<float> Result = Vector128.Multiply(A.AsVector128(), Vector128.Create(B));
        return new Vector4F(Result.GetElement(0), Result.GetElement(1), Result.GetElement(2), Result.GetElement(3));
    }
        
    // ----------------------------------------------------------------------

    public Vector4F Negative => new Vector4F(-X, -Y, -Z, -W);

    public Vector4F Normalized{
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get{
            float LS = X * X + Y * Y + Z * Z + W * W;
            if(LS == 0 || float.IsNaN(LS)){ return new Vector4F(); }
            float IL = 1f / System.MathF.Sqrt(LS);
            return new Vector4F(X * IL, Y * IL, Z * IL, W * IL);
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4F operator -(Vector4F A) => A.Negative;
    
    public float Length{
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => System.MathF.Sqrt(X * X + Y * Y + Z * Z + W * W);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Dot(Vector4F A, Vector4F B) => (A.X * B.X) + (A.Y * B.Y) + (A.Z * B.Z) + (A.W * B.W);
    
    // ----------------------------------------------------------------------

    public static Vector4F Zero => new Vector4F(0);
    public static Vector4F One => new Vector4F(1);
    public static Vector4F Right => new Vector4F(1, 0, 0, 0);
    public static Vector4F Up => new Vector4F(0, 1, 0, 0);
    public static Vector4F Front => new Vector4F(0, 0, 1, 0);
    public static Vector4F Ana => new Vector4F(0, 0, 0, 1);
    public static Vector4F MaxValue => new Vector4F(float.MaxValue);
    public static Vector4F MinValue => new Vector4F(float.MinValue);
    
    // ----------------------------------------------------------------------
    
    public Dictionary<string, object?> __Pack() => new Dictionary<string, object?>{
        ["XYZW"] = $"{X}|{Y}|{Z}|{W}"
    };

    public void __Unpack(Dictionary<string, object?> Data){
        string XYZ = WL.Packer.Get<string>(Data, "XYZW", "0|0|0|0")!;

        string[] Parts = XYZ.Split("|");
        if(Parts.Length >= 4){
            float.TryParse(Parts[0], out X);
            float.TryParse(Parts[1], out Y);
            float.TryParse(Parts[2], out Z);
            float.TryParse(Parts[3], out W);
        }
    }
    
    // ----------------------------------------------------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Vector4F Other) => X == Other.X && Y == Other.Y && Z == Other.Z && W == Other.W;

    public override bool Equals(object? Object) => Object is Vector4F Other && Equals(Other);

    public override int GetHashCode() => HashCode.Combine(X, Y, Z, W);

    public static bool operator ==(Vector4F L, Vector4F R) =>  L.Equals(R);
    public static bool operator !=(Vector4F L, Vector4F R) => !L.Equals(R);

    public override string ToString() => $"Vector4F({X}, {Y}, {Z}, {W})";
}