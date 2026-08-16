using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace WLO.Math;

public readonly struct Vector2I : IEquatable<Vector2I>{
    public readonly int X;
    public readonly int Y;

    public int W => X;
    public int H => Y;

    public Vector2I(int X, int Y){
        this.X = X;
        this.Y = Y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Vector128<int> AsVector128() => Vector128.Create(X, Y, 0, 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2I operator +(Vector2I A, Vector2I B){
        Vector128<int> Result = Vector128.Add(A.AsVector128(), B.AsVector128());
        return new Vector2I(Result.GetElement(0), Result.GetElement(1));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2I operator -(Vector2I A, Vector2I B){
        Vector128<int> Result = Vector128.Subtract(A.AsVector128(), B.AsVector128());
        return new Vector2I(Result.GetElement(0), Result.GetElement(1));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2I operator *(Vector2I A, Vector2I B){
        Vector128<int> Result = Vector128.Multiply(A.AsVector128(), B.AsVector128());
        return new Vector2I(Result.GetElement(0), Result.GetElement(1));
    }
    
    // ----------------------------------------------------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Vector2I Other) => X == Other.X && Y == Other.Y;

    public override bool Equals(object? Object) => Object is Vector2I Other && Equals(Other);

    public override int GetHashCode() => HashCode.Combine(X, Y);

    public static bool operator ==(Vector2I L, Vector2I R) =>  L.Equals(R);
    public static bool operator !=(Vector2I L, Vector2I R) => !L.Equals(R);

    public override string ToString() => $"Vector2I({X}, {Y})";
}