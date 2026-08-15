using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace WLO.Math;

public readonly struct Vector2F{
    public readonly float X;
    public readonly float Y;

    public float W => X;
    public float H => Y;

    public Vector2F(float X, float Y){
        this.X = X;
        this.Y = Y;
    }

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
}