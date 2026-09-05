using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace WLO.Math;

public struct Vector2F : IEquatable<Vector2F>, WLI.Packable{
    public float X;
    public float Y;

    public float W => X;
    public float H => Y;

    public Vector2F(float X, float Y){
        this.X = X;
        this.Y = Y;
    }

    public Vector3F To3F() => new Vector3F(X, Y, 0);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Vector128<float> AsVector128() => Vector128.Create(X, Y, 0, 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2F operator +(Vector2F A, Vector2F B){
        Vector128<float> Result = Vector128.Add(A.AsVector128(), B.AsVector128());
        return new Vector2F(Result.GetElement(0), Result.GetElement(1));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2F operator -(Vector2F A, Vector2F B){
        Vector128<float> Result = Vector128.Subtract(A.AsVector128(), B.AsVector128());
        return new Vector2F(Result.GetElement(0), Result.GetElement(1));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2F operator *(Vector2F A, Vector2F B){
        Vector128<float> Result = Vector128.Multiply(A.AsVector128(), B.AsVector128());
        return new Vector2F(Result.GetElement(0), Result.GetElement(1));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2F operator +(Vector2F A, float B){
        Vector128<float> Result = Vector128.Add(A.AsVector128(), Vector128.Create(B));
        return new Vector2F(Result.GetElement(0), Result.GetElement(1));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2F operator *(Vector2F A, float B){
        Vector128<float> Result = Vector128.Multiply(A.AsVector128(), Vector128.Create(B));
        return new Vector2F(Result.GetElement(0), Result.GetElement(1));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vector2F(System.Numerics.Vector2 V) => new Vector2F(V.X, V.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator System.Numerics.Vector2(Vector2F V) => new System.Numerics.Vector2(V.X, V.Y);
    
    // ----------------------------------------------------------------------
    
    public Dictionary<string, object?> __Pack() => new Dictionary<string, object?>{
        ["XY"] = $"{X}|{Y}"
    };

    public void __Unpack(Dictionary<string, object?> Data){
        string XY = WL.Packer.Get<string>(Data, "XY", "0|0")!;

        string[] Parts = XY.Split("|");
        if(Parts.Length >= 3){
            float.TryParse(Parts[0], out X);
            float.TryParse(Parts[1], out Y);
        }
    }
    
    // ----------------------------------------------------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Vector2F Other) => X == Other.X && Y == Other.Y;

    public override bool Equals(object? Object) => Object is Vector2F Other && Equals(Other);

    public override int GetHashCode() => HashCode.Combine(X, Y);

    public static bool operator ==(Vector2F L, Vector2F R) =>  L.Equals(R);
    public static bool operator !=(Vector2F L, Vector2F R) => !L.Equals(R);

    public override string ToString() => $"Vector2F({X}, {Y})";
}