using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace WLO.Math;

public struct Vector3F : IEquatable<Vector3F>, WLI.Packable{
    public float X;
    public float Y;
    public float Z;

    public float W{
        get => X;
        set => X = value;
    }
    public float H{
        get => Y;
        set => Y = value;
    }
    public float D{
        get => Z;
        set => Z = value;
    }

    public float Pitch{
        get => X;
        set => X = value;
    }
    public float Yaw{
        get => Y;
        set => Y = value;
    }
    public float Roll{
        get => Z;
        set => Z = value;
    }
    
    public Vector3F(float X, float Y, float Z){
        this.X = X;
        this.Y = Y;
        this.Z = Z;
    }
    public Vector3F(float XYZ) : this(XYZ, XYZ, XYZ){}

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Vector128<float> AsVector128() => Vector128.Create(X, Y, Z, 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3F operator +(Vector3F A, Vector3F B){
        Vector128<float> Result = Vector128.Add(A.AsVector128(), B.AsVector128());
        return new Vector3F(Result.GetElement(0), Result.GetElement(1), Result.GetElement(2));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3F operator -(Vector3F A, Vector3F B){
        Vector128<float> Result = Vector128.Subtract(A.AsVector128(), B.AsVector128());
        return new Vector3F(Result.GetElement(0), Result.GetElement(1), Result.GetElement(2));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3F operator *(Vector3F A, Vector3F B){
        Vector128<float> Result = Vector128.Multiply(A.AsVector128(), B.AsVector128());
        return new Vector3F(Result.GetElement(0), Result.GetElement(1), Result.GetElement(2));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3F operator +(Vector3F A, float B){
        Vector128<float> Result = Vector128.Add(A.AsVector128(), Vector128.Create(B));
        return new Vector3F(Result.GetElement(0), Result.GetElement(1), Result.GetElement(2));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3F operator *(Vector3F A, float B){
        Vector128<float> Result = Vector128.Multiply(A.AsVector128(), Vector128.Create(B));
        return new Vector3F(Result.GetElement(0), Result.GetElement(1), Result.GetElement(2));
    }
        
    // ----------------------------------------------------------------------

    public Vector3F Negative => new Vector3F(-X, -Y, -Z);

    public Vector3F Normalized{
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get{
            float LS = X * X + Y * Y + Z * Z;
            if(LS == 0 || float.IsNaN(LS)){ return new Vector3F(); }
            float IL = 1f / System.MathF.Sqrt(LS);
            return new Vector3F(X * IL, Y * IL, Z * IL);
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3F Cross(Vector3F A, Vector3F B){
        Vector128<float> A1 = Vector128.Create(A.Y, A.Z, A.X, 0f);
        Vector128<float> B1 = Vector128.Create(B.Z, B.X, B.Y, 0f);
    
        Vector128<float> A2 = Vector128.Create(A.Z, A.X, A.Y, 0f);
        Vector128<float> B2 = Vector128.Create(B.Y, B.Z, B.X, 0f);
    
        Vector128<float> Result = Vector128.Subtract(
            Vector128.Multiply(A1, B1),
            Vector128.Multiply(A2, B2)
        );
    
        return new Vector3F(
            Result.GetElement(0),
            Result.GetElement(1),
            Result.GetElement(2)
        );
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3F operator -(Vector3F A) => A.Negative;
    
    public float Length{
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => System.MathF.Sqrt(X * X + Y * Y + Z * Z);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Dot(Vector3F A, Vector3F B) => (A.X * B.X) + (A.Y * B.Y) + (A.Z * B.Z);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vector3F(System.Numerics.Vector3 V) => new Vector3F(V.X, V.Y, V.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator System.Numerics.Vector3(Vector3F V) => new System.Numerics.Vector3(V.X, V.Y, V.Z);
    
    // ----------------------------------------------------------------------

    public static Vector3F Zero => new Vector3F(0);
    public static Vector3F One => new Vector3F(1);
    public static Vector3F Right => new Vector3F(1, 0, 0);
    public static Vector3F Up => new Vector3F(0, 1, 0);
    public static Vector3F Front => new Vector3F(0, 0, 1); // todo, НУ ВОТ НУ ВОТ ГОВНО НУ ВОТ ЖЕ, OpenGL шлюха!!! почему +z это на пользователя из монитора??? чё вообще за хуйню они несут? я привык, все привыкли, что +z это вперёд!!!! а не назад!!!
    public static Vector3F MaxValue => new Vector3F(float.MaxValue);
    public static Vector3F MinValue => new Vector3F(float.MinValue);
    
    // ----------------------------------------------------------------------
    
    public Dictionary<string, object?> __Pack() => new Dictionary<string, object?>{
        ["XYZ"] = $"{X}|{Y}|{Z}"
    };

    public void __Unpack(Dictionary<string, object?> Data){
        string XYZ = WL.Packer.Get<string>(Data, "XYZ", "0|0|0")!;

        string[] Parts = XYZ.Split("|");
        if(Parts.Length >= 3){
            float.TryParse(Parts[0], out X);
            float.TryParse(Parts[1], out Y);
            float.TryParse(Parts[2], out Z);
        }
    }
    
    // ----------------------------------------------------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Vector3F Other) => X == Other.X && Y == Other.Y && Z == Other.Z;

    public override bool Equals(object? Object) => Object is Vector3F Other && Equals(Other);

    public override int GetHashCode() => HashCode.Combine(X, Y, Z);

    public static bool operator ==(Vector3F L, Vector3F R) =>  L.Equals(R);
    public static bool operator !=(Vector3F L, Vector3F R) => !L.Equals(R);

    public override string ToString() => $"Vector3F({X}, {Y}, {Z})";
}