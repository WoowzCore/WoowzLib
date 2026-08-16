using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace WLO.Math;

public readonly struct Vector3F : IEquatable<Vector3F>{
    public readonly float X;
    public readonly float Y;
    public readonly float Z;

    public float W => X;
    public float H => Y;
    public float D => Z;

    public Vector3F(float X, float Y, float Z){
        this.X = X;
        this.Y = Y;
        this.Z = Z;
    }

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Vector3F Other) => X == Other.X && Y == Other.Y && Z == Other.Z;

    public override bool Equals(object? Object) => Object is Vector3F Other && Equals(Other);

    public override int GetHashCode() => HashCode.Combine(X, Y);

    public static bool operator ==(Vector3F L, Vector3F R) =>  L.Equals(R);
    public static bool operator !=(Vector3F L, Vector3F R) => !L.Equals(R);

    public override string ToString() => $"Vector3F({X}, {Y}, {Z})";
}