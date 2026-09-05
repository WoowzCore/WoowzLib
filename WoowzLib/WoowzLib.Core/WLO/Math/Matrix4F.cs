using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace WLO.Math;

[StructLayout(LayoutKind.Sequential)]
public readonly struct Matrix4F : IEquatable<Matrix4F>{
    private readonly Vector128<float> __Column1;
    private readonly Vector128<float> __Column2;
    private readonly Vector128<float> __Column3;
    private readonly Vector128<float> __Column4;

    public Matrix4F(Vector128<float> C1, Vector128<float> C2, Vector128<float> C3, Vector128<float> C4){ __Column1 = C1; __Column2 = C2; __Column3 = C3; __Column4 = C4; }
    
    // принимает column-major (для графики самое то)
    public Matrix4F(float C0R0, float C0R1, float C0R2, float C0R3, float C1R0, float C1R1, float C1R2, float C1R3, float C2R0, float C2R1, float C2R2, float C2R3, float C3R0, float C3R1, float C3R2, float C3R3) : this(
        Vector128.Create(C0R0, C0R1, C0R2, C0R3),
        Vector128.Create(C1R0, C1R1, C1R2, C1R3),
        Vector128.Create(C2R0, C2R1, C2R2, C2R3),
        Vector128.Create(C3R0, C3R1, C3R2, C3R3)
    ){}
    
    // принимает row-major
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
                0 => __Column1.GetElement(Row),
                1 => __Column2.GetElement(Row),
                2 => __Column3.GetElement(Row),
                3 => __Column4.GetElement(Row),
                var _ => throw new IndexOutOfRangeException()
            };
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix4F operator *(Matrix4F A, Matrix4F B){
        return new Matrix4F(
            MultiplyColumn(A, B.__Column1),
            MultiplyColumn(A, B.__Column2),
            MultiplyColumn(A, B.__Column3),
            MultiplyColumn(A, B.__Column4)
        );
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<float> MultiplyColumn(Matrix4F Matrix, Vector128<float> Column){
        Vector128<float> Result = Vector128.Multiply(Matrix.__Column1, Vector128.Create(Column.GetElement(0)));
        Result = Vector128.Add(Result, Vector128.Multiply(Matrix.__Column2, Vector128.Create(Column.GetElement(1))));
        Result = Vector128.Add(Result, Vector128.Multiply(Matrix.__Column3, Vector128.Create(Column.GetElement(2))));
        Result = Vector128.Add(Result, Vector128.Multiply(Matrix.__Column4, Vector128.Create(Column.GetElement(3))));
        return Result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3F operator *(Matrix4F Matrix, Vector3F Vector) {
        Vector128<float> Vector__ = Vector128.Create(Vector.X, Vector.Y, Vector.Z, 1.0f);
        Vector128<float> Result = Vector128.Multiply(Matrix.__Column1, Vector__.GetElement(0));
        Result = Vector128.Add(Result, Vector128.Multiply(Matrix.__Column2, Vector__.GetElement(1)));
        Result = Vector128.Add(Result, Vector128.Multiply(Matrix.__Column3, Vector__.GetElement(2)));
        Result = Vector128.Add(Result, Vector128.Multiply(Matrix.__Column4, Vector__.GetElement(3)));
        
        return new Vector3F(Result.GetElement(0), Result.GetElement(1), Result.GetElement(2));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3F TransformNormal(Vector3F Vector){
        Vector128<float> Vector__ = Vector128.Create(Vector.X, Vector.Y, Vector.Z, 0.0f);
        Vector128<float> Result = Vector128.Multiply(__Column1, Vector__.GetElement(0));
        Result = Vector128.Add(Result, Vector128.Multiply(__Column2, Vector__.GetElement(1)));
        Result = Vector128.Add(Result, Vector128.Multiply(__Column3, Vector__.GetElement(2)));
        
        return new Vector3F(Result.GetElement(0), Result.GetElement(1), Result.GetElement(2));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix4F CreateLookAt(Vector3F Eye, Vector3F Target, Vector3F Up) {
        Vector3F Z = (Eye - Target).Normalized;
        Vector3F X = Vector3F.Cross(Up, Z).Normalized;
        Vector3F Y = Vector3F.Cross(Z, X);

        return new Matrix4F(
            X.X, Y.X, Z.X, 0,
            X.Y, Y.Y, Z.Y, 0,
            X.Z, Y.Z, Z.Z, 0,
            -Vector3F.Dot(X, Eye),
            -Vector3F.Dot(Y, Eye),
            -Vector3F.Dot(Z, Eye),
            1
        );
    }

    public Vector3F Translation{
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new Vector3F(__Column4.GetElement(0), __Column4.GetElement(1), __Column4.GetElement(2));
    }
    
    public static Matrix4F Identity => new Matrix4F(Vector128.Create(1f, 0, 0, 0), Vector128.Create(0, 1f, 0, 0), Vector128.Create(0, 0, 1f, 0), Vector128.Create(0, 0, 0, 1f));
    
    // ----------------------------------------------------------------------

    // TODO ALL!!!! also add aggressive inlining
    
    public static Matrix4F CreatePerspective(float FOVRadians, float Aspect, float ZNear, float ZFar){
        float F = 1f / (float)System.Math.Tan(FOVRadians / 2f);
        return new Matrix4F(
            F / Aspect, 0, 0, 0,
            0, F, 0, 0,
            0, 0, (ZFar + ZNear) / (ZNear - ZFar), -1,
            0, 0, (2 * ZFar * ZNear) / (ZNear - ZFar), 0
        );
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix4F CreateOrtho(float Left, float Right, float Bottom, float Top, float ZNear, float ZFar){
        float RL = Right - Left;
        float TB = Top - Bottom;
        float FN = ZFar - ZNear;

        return new Matrix4F(
            Vector128.Create(2.0f / RL, 0, 0, 0),
            Vector128.Create(0, 2.0f / TB, 0, 0),
            Vector128.Create(0, 0, -2.0f / FN, 0),
            Vector128.Create(
                -(Right + Left) / RL, 
                -(Top + Bottom) / TB, 
                -(ZFar + ZNear) / FN, 
                1.0f)
        );
    }

    public static Matrix4F CreateTranslation(Vector3F Position) => new Matrix4F(
        1, 0, 0, 0,
        0, 1, 0, 0,
        0, 0, 1, 0,
        Position.X, Position.Y, Position.Z, 1
    );

    public static Matrix4F CreateScale(Vector3F Scale) => new Matrix4F(
        Scale.W, 0, 0, 0,
        0, Scale.H, 0, 0,
        0, 0, Scale.D, 0,
        0, 0, 0, 1
    );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix4F CreateRotation(Vector3F Rotation) => CreateRotationPitch(Rotation.Pitch) * CreateRotationYaw(Rotation.Yaw) * CreateRotationRoll(Rotation.Roll);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix4F CreateRotationPitch(float Radians){
        float Cos = (float)System.Math.Cos(Radians);
        float Sin = (float)System.Math.Sin(Radians);
        return new Matrix4F(
            1, 0, 0, 0,
            0, Cos, Sin, 0,
            0, -Sin, Cos, 0,
            0, 0, 0, 1
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix4F CreateRotationYaw(float Radians){
        float Cos = (float)System.Math.Cos(Radians);
        float Sin = (float)System.Math.Sin(Radians);
        return new Matrix4F(
            Cos, 0, -Sin, 0,
            0, 1, 0, 0,
            Sin, 0, Cos, 0,
            0, 0, 0, 1
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix4F CreateRotationRoll(float Radians){
        float Cos = (float)System.Math.Cos(Radians);
        float Sin = (float)System.Math.Sin(Radians);
        return new Matrix4F(
            Cos, Sin, 0, 0,
            -Sin, Cos, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1
        );
    }
    
    // ----------------------------------------------------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Matrix4F Other){
        return Vector128.EqualsAll(__Column1, Other.__Column1) &&
               Vector128.EqualsAll(__Column2, Other.__Column2) &&
               Vector128.EqualsAll(__Column3, Other.__Column3) &&
               Vector128.EqualsAll(__Column4, Other.__Column4);
    }

    public override bool Equals(object? Object) => Object is Matrix4F Other && Equals(Other);

    public override int GetHashCode() => HashCode.Combine(__Column1, __Column2, __Column3, __Column4);

    public static bool operator ==(Matrix4F L, Matrix4F R) =>  L.Equals(R);
    public static bool operator !=(Matrix4F L, Matrix4F R) => !L.Equals(R);

    public override string ToString() => $"Matrix4F(todo)";
}