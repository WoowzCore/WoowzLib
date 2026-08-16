using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace WLO.Math;

[StructLayout(LayoutKind.Sequential)]
public readonly struct Matrix4F : IEquatable<Matrix4F>{
    private readonly Vector128<float> __C1;
    private readonly Vector128<float> __C2;
    private readonly Vector128<float> __C3;
    private readonly Vector128<float> __C4;

    public Matrix4F(Vector128<float> C1, Vector128<float> C2, Vector128<float> C3, Vector128<float> C4){ __C1 = C1; __C2 = C2; __C3 = C3; __C4 = C4; }

    public static Matrix4F FromRows(float R0C0, float R0C1, float R0C2, float R0C3, float R1C0, float R1C1, float R1C2, float R1C3, float R2C0, float R2C1, float R2C2, float R2C3, float R3C0, float R3C1, float R3C2, float R3C3){
        return new Matrix4F(
            Vector128.Create(R0C0, R1C0, R2C0, R3C0),
            Vector128.Create(R0C1, R1C1, R2C1, R3C1),
            Vector128.Create(R0C2, R1C2, R2C2, R3C2),
            Vector128.Create(R0C3, R1C3, R2C3, R3C3)
        );
    }

    public float this[int Row, int Column]{
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get{
            return Column switch{
                0 => __C1.GetElement(Row),
                1 => __C2.GetElement(Row),
                2 => __C3.GetElement(Row),
                3 => __C4.GetElement(Row),
                var _ => throw new IndexOutOfRangeException()
            };
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix4F operator *(Matrix4F A, Matrix4F B){
        return new Matrix4F(
            MultiplyColumn(A, B.__C1),
            MultiplyColumn(A, B.__C2),
            MultiplyColumn(A, B.__C3),
            MultiplyColumn(A, B.__C4)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<float> MultiplyColumn(Matrix4F Matrix, Vector128<float> Column){
        Vector128<float> Result = Vector128.Multiply(Matrix.__C1, Vector128.Create(Column.GetElement(0)));
        Result = Vector128.Add(Result, Vector128.Multiply(Matrix.__C2, Vector128.Create(Column.GetElement(1))));
        Result = Vector128.Add(Result, Vector128.Multiply(Matrix.__C3, Vector128.Create(Column.GetElement(2))));
        Result = Vector128.Add(Result, Vector128.Multiply(Matrix.__C4, Vector128.Create(Column.GetElement(3))));
        return Result;
    }

    public static Matrix4F Identity => new Matrix4F(Vector128.Create(1f, 0, 0, 0), Vector128.Create(0, 1f, 0, 0), Vector128.Create(0, 0, 1f, 0), Vector128.Create(0, 0, 0, 1f));
    
    // ----------------------------------------------------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Matrix4F Other){
        return Vector128.EqualsAll(__C1, Other.__C1) &&
               Vector128.EqualsAll(__C2, Other.__C2) &&
               Vector128.EqualsAll(__C3, Other.__C3) &&
               Vector128.EqualsAll(__C4, Other.__C4);
    }

    public override bool Equals(object? Object) => Object is Matrix4F Other && Equals(Other);

    public override int GetHashCode() => HashCode.Combine(__C1, __C2, __C3, __C4);

    public static bool operator ==(Matrix4F L, Matrix4F R) =>  L.Equals(R);
    public static bool operator !=(Matrix4F L, Matrix4F R) => !L.Equals(R);

    public override string ToString() => $"Matrix4F(todo)";
}