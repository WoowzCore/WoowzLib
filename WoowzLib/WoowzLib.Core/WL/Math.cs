using System.Runtime.CompilerServices;
using WLO.Math;

namespace WL;

public struct Math{
    public const float PiF      = System.MathF.PI;
    public const float DegToRad = PiF / 180;
    public const float RadToDeg = 180 / PiF;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ClampF(float Value, float Min, float Max) => Value < Min ? Min : (Value > Max ? Max : Value);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float LerpSafeF(float A, float B, float T) => A + (B - A) * ClampF(T, 0, 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float LerpF(float A, float B, float T) => A + (B - A) * T;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3F LerpV3F(Vector3F A, Vector3F B, float T) => new Vector3F(LerpF(A.X, B.X, T), LerpF(A.Y, B.Y, T), LerpF(A.Z, B.Z, T));
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3F LerpSafeV3F(Vector3F A, Vector3F B, float T) => new Vector3F(LerpSafeF(A.X, B.X, T), LerpSafeF(A.Y, B.Y, T), LerpSafeF(A.Z, B.Z, T));

    public static Vector3F MatrixToScale(Matrix4F M){
        float SX = new Vector3F(M[0, 0], M[1, 0], M[2, 0]).Length;
        float SY = new Vector3F(M[0, 1], M[1, 1], M[2, 1]).Length;
        float SZ = new Vector3F(M[0, 2], M[1, 2], M[2, 2]).Length;
        return new Vector3F(SX, SY, SZ);
    }

    public static Vector3F MatrixToRotation(Matrix4F M){
        Vector3F Scale = MatrixToScale(M);
        if(Scale.X == 0 || Scale.Y == 0 || Scale.Z == 0){ return Vector3F.Zero; }

        // TODO, лишние счёта, перенетсти в условия
        float M00 = M[0, 0] / Scale.X; float M01 = M[0, 1] / Scale.Y; float M02 = M[0, 2] / Scale.Z;
        float M11 = M[1, 1] / Scale.Y; float M21 = M[2, 1] / Scale.Y;
        float M12 = M[1, 2] / Scale.Z; float M22 = M[2, 2] / Scale.Z;

        float Pitch, Yaw, Roll;
        Yaw = System.MathF.Asin(ClampF(-M02, -1, 1));

        if(System.MathF.Abs(M02) < 0.99999f){
            Pitch = System.MathF.Atan2(M12, M22);
            Roll  = System.MathF.Atan2(M01, M00);
        }else{
            Pitch = System.MathF.Atan2(-M21, M11);
            Roll = 0;
        }

        return new Vector3F(Pitch, Yaw, Roll);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Expand3D(ref Vector3F Min, ref Vector3F Max, Vector3F Position){
        if(Position.X < Min.X){ Min.X = Position.X; }
        if(Position.Y < Min.Y){ Min.Y = Position.Y; }
        if(Position.Z < Min.Z){ Min.Z = Position.Z; }
            
        if(Position.X > Max.X){ Max.X = Position.X; }
        if(Position.Y > Max.Y){ Max.Y = Position.Y; }
        if(Position.Z > Max.Z){ Max.Z = Position.Z; }
    }
    
    /// Делает массив чисел уникальными
    public static void GenerateSequential(Span<uint> Buffer, uint Start = 0){
        for(int i = 0; i < Buffer.Length; i++){ Buffer[i] = Start + (uint)i; }
    }
}