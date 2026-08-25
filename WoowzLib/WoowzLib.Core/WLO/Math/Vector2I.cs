using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace WLO.Math;

public struct Vector2I : IEquatable<Vector2I>, WLI.Packable{
    public int X;
    public int Y;

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
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2I operator +(Vector2I A, int B){
        Vector128<int> Result = Vector128.Add(A.AsVector128(), Vector128.Create(B));
        return new Vector2I(Result.GetElement(0), Result.GetElement(1));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2I operator *(Vector2I A, int B){
        Vector128<int> Result = Vector128.Multiply(A.AsVector128(), Vector128.Create(B));
        return new Vector2I(Result.GetElement(0), Result.GetElement(1));
    }
    
    // ----------------------------------------------------------------------

    public float Aspect => (float)X / Y;

    // ----------------------------------------------------------------------
    
    public Dictionary<string, object?> __Pack() => new Dictionary<string, object?>{
        ["XY"] = $"{X}|{Y}"
    };

    public void __Unpack(Dictionary<string, object?> Data){
        string XY = WL.Packer.Get<string>(Data, "XY", "0|0")!;

        string[] Parts = XY.Split("|");
        if(Parts.Length >= 3){
            int.TryParse(Parts[0], out X);
            int.TryParse(Parts[1], out Y);
        }
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