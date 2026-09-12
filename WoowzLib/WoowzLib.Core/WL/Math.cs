using System.Runtime.CompilerServices;
using WLO.Math;

namespace WL;

public struct Math{
    public const float  PiF = System.MathF.PI;
    public const double PiD = System.Math.PI;

    public const float  MaxValueF = float.MaxValue;
    public const int    MaxValueI = int.MaxValue;
    public const double MaxValueD = double.MaxValue;
    public const float  MinValueF = float.MinValue;
    public const int    MinValueI = int.MinValue;
    public const double MinValueD = double.MinValue;

    public const float  NANF = float .NaN;
    public const double NAND = double.NaN;
    
    public const float  DegToRadF = PiF / 180;
    public const float  RadToDegF = 180 / PiF;
    public const double DegToRadD = PiD / 180;
    public const double RadToDegD = 180 / PiD;

    public const float  EpsilonF = 1e-6f;
    public const double EpsilonD = 1e-6;
    
    public const float  Inverse255 = 1f / 255;
    
    // ----------------------------------------------------------------------
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float  MinF(float  A, float  B) => System.Math.Min(A, B);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int    MinI(int    A, int    B) => System.Math.Min(A, B);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double MinD(double A, double B) => System.Math.Min(A, B);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float  MaxF(float  A, float  B) => System.Math.Max(A, B);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int    MaxI(int    A, int    B) => System.Math.Max(A, B);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double MaxD(double A, double B) => System.Math.Max(A, B);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float  ClampF(float  A, float  Min, float  Max) => System.Math.Clamp(A, Min, Max);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int    ClampI(int    A, int    Min, int    Max) => System.Math.Clamp(A, Min, Max);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double ClampD(double A, double Min, double Max) => System.Math.Clamp(A, Min, Max);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float  Clamp01F(float  A) => ClampF(A, 0, 1);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int    Clamp01I(int    A) => ClampI(A, 0, 1);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double Clamp01D(double A) => ClampD(A, 0, 1);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float  LerpF(float  A, float  B, float  T) => float .Lerp(A, B, T);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int    LerpI(int    A, int    B, float  T) => RoundI(float.Lerp(A, B, T));
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double LerpD(double A, double B, double T) => double.Lerp(A, B, T);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float  LerpSafeF(float  A, float  B, float  T) => LerpF(A, B, Clamp01F(T));
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int    LerpSafeI(int    A, int    B, float  T) => LerpI(A, B, Clamp01F(T));
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double LerpSafeD(double A, double B, double T) => LerpD(A, B, Clamp01D(T));
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float  FloorF(float  A) => System.MathF.Floor(A);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int    FloorI(float  A) => (int)FloorF(A);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int    FloorI(double A) => (int)FloorD(A);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double FloorD(double A) => System.Math.Floor(A);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float  RoundF(float  A) => System.MathF.Round(A);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int    RoundI(float  A) => (int)RoundF(A);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int    RoundI(double A) => (int)RoundD(A);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double RoundD(double A) => System.Math.Round(A);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float  CeilF(float  A) => System.MathF.Ceiling(A);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int    CeilI(float  A) => (int)CeilF(A);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int    CeilI(double A) => (int)CeilD(A);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double CeilD(double A) => System.Math.Ceiling(A);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float  AbsF(float  A) => System.Math.Abs(A);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int    AbsI(int    A) => System.Math.Abs(A);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double AbsD(double A) => System.Math.Abs(A);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float  SqrtF(float  A) => System.MathF.Sqrt(A);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float  SqrtI(int    A) => System.MathF.Sqrt(A);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double SqrtD(double A) => System.Math .Sqrt(A);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float  AspectF(float  W, float  H) => H > 0 ? W/H : 1;
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float  AspectI(int    W, int    H) => AspectF(W, H);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double AspectD(double W, double H) => H > 0 ? W/H : 1;
    
    // ----------------------------------------------------------------------
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float  LengthSquared2F(float  X, float  Y) => X*X + Y*Y;
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int    LengthSquared2I(int    X, int    Y) => X*X + Y*Y;
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double LengthSquared2D(double X, double Y) => X*X + Y*Y;
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float  LengthSquared3F(float  X, float  Y, float  Z) => X*X + Y*Y + Z*Z;
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int    LengthSquared3I(int    X, int    Y, int    Z) => X*X + Y*Y + Z*Z;
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double LengthSquared3D(double X, double Y, double Z) => X*X + Y*Y + Z*Z;
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float  LengthSquared4F(float  X, float  Y, float  Z, float  W) => X*X + Y*Y + Z*Z + W*W;
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int    LengthSquared4I(int    X, int    Y, int    Z, int    W) => X*X + Y*Y + Z*Z + W*W;
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double LengthSquared4D(double X, double Y, double Z, double W) => X*X + Y*Y + Z*Z + W*W;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float  Length2F(float  X, float  Y) => SqrtF(LengthSquared2F(X, Y));
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float  Length2I(int    X, int    Y) => SqrtI(LengthSquared2I(X, Y));
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double Length2D(double X, double Y) => SqrtD(LengthSquared2D(X, Y));
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float  Length3F(float  X, float  Y, float  Z) => SqrtF(LengthSquared3F(X, Y, Z));
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float  Length3I(int    X, int    Y, int    Z) => SqrtI(LengthSquared3I(X, Y, Z));
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double Length3D(double X, double Y, double Z) => SqrtD(LengthSquared3D(X, Y, Z));
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float  Length4F(float  X, float  Y, float  Z, float  W) => SqrtF(LengthSquared4F(X, Y, Z, W));
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float  Length4I(int    X, int    Y, int    Z, int    W) => SqrtI(LengthSquared4I(X, Y, Z, W));
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double Length4D(double X, double Y, double Z, double W) => SqrtD(LengthSquared4D(X, Y, Z, W));
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float  Dot2F(float  AX, float  AY, float  BX, float  BY) => (AX*BX) + (AY*BY);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int    Dot2I(int    AX, int    AY, int    BX, int    BY) => (AX*BX) + (AY*BY);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double Dot2D(double AX, double AY, double BX, double BY) => (AX*BX) + (AY*BY);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float  Dot3F(float  AX, float  AY, float  AZ, float  BX, float  BY, float  BZ) => (AX*BX) + (AY*BY) + (AZ*BZ);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int    Dot3I(int    AX, int    AY, int    AZ, int    BX, int    BY, int    BZ) => (AX*BX) + (AY*BY) + (AZ*BZ);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double Dot3D(double AX, double AY, double AZ, double BX, double BY, double BZ) => (AX*BX) + (AY*BY) + (AZ*BZ);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float  Dot4F(float  AX, float  AY, float  AZ, float  AW, float  BX, float  BY, float  BZ, float  BW) => (AX*BX) + (AY*BY) + (AZ*BZ) + (AW*BW);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int    Dot4I(int    AX, int    AY, int    AZ, int    AW, int    BX, int    BY, int    BZ, int    BW) => (AX*BX) + (AY*BY) + (AZ*BZ) + (AW*BW);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double Dot4D(double AX, double AY, double AZ, double AW, double BX, double BY, double BZ, double BW) => (AX*BX) + (AY*BY) + (AZ*BZ) + (AW*BW);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float  CrossF(float  AX, float  AY, float  BX, float  BY) => (AX*BY) - (AY*BX);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int    CrossI(int    AX, int    AY, int    BX, int    BY) => (AX*BY) - (AY*BX);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double CrossD(double AX, double AY, double BX, double BY) => (AX*BY) - (AY*BX);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float  DistanceSquared2F(float  AX, float  AY, float  BX, float  BY) => LengthSquared2F(AX-BX, AY-BY);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int    DistanceSquared2I(int    AX, int    AY, int    BX, int    BY) => LengthSquared2I(AX-BX, AY-BY);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double DistanceSquared2D(double AX, double AY, double BX, double BY) => LengthSquared2D(AX-BX, AY-BY);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float  DistanceSquared3F(float  AX, float  AY, float  AZ, float  BX, float  BY, float  BZ) => LengthSquared3F(AX-BX, AY-BY, AZ-BZ);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int    DistanceSquared3I(int    AX, int    AY, int    AZ, int    BX, int    BY, int    BZ) => LengthSquared3I(AX-BX, AY-BY, AZ-BZ);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double DistanceSquared3D(double AX, double AY, double AZ, double BX, double BY, double BZ) => LengthSquared3D(AX-BX, AY-BY, AZ-BZ);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float  DistanceSquared4F(float  AX, float  AY, float  AZ, float  AW, float  BX, float  BY, float  BZ, float  BW) => LengthSquared4F(AX-BX, AY-BY, AZ-BZ, AW-BW);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int    DistanceSquared4I(int    AX, int    AY, int    AZ, int    AW, int    BX, int    BY, int    BZ, int    BW) => LengthSquared4I(AX-BX, AY-BY, AZ-BZ, AW-BW);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double DistanceSquared4D(double AX, double AY, double AZ, double AW, double BX, double BY, double BZ, double BW) => LengthSquared4D(AX-BX, AY-BY, AZ-BZ, AW-BW);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float  Distance2F(float  AX, float  AY, float  BX, float  BY) => Length2F(AX-BX, AY-BY);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float  Distance2I(int    AX, int    AY, int    BX, int    BY) => Length2I(AX-BX, AY-BY);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double Distance2D(double AX, double AY, double BX, double BY) => Length2D(AX-BX, AY-BY);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float  Distance3F(float  AX, float  AY, float  AZ, float  BX, float  BY, float  BZ) => Length3F(AX-BX, AY-BY, AZ-BZ);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float  Distance3I(int    AX, int    AY, int    AZ, int    BX, int    BY, int    BZ) => Length3I(AX-BX, AY-BY, AZ-BZ);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double Distance3D(double AX, double AY, double AZ, double BX, double BY, double BZ) => Length3D(AX-BX, AY-BY, AZ-BZ);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float  Distance4F(float  AX, float  AY, float  AZ, float  AW, float  BX, float  BY, float  BZ, float  BW) => Length4F(AX-BX, AY-BY, AZ-BZ, AW-BW);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float  Distance4I(int    AX, int    AY, int    AZ, int    AW, int    BX, int    BY, int    BZ, int    BW) => Length4I(AX-BX, AY-BY, AZ-BZ, AW-BW);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double Distance4D(double AX, double AY, double AZ, double AW, double BX, double BY, double BZ, double BW) => Length4D(AX-BX, AY-BY, AZ-BZ, AW-BW);
    
    // ----------------------------------------------------------------------
    
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
    public static QuaternionF SlerpSafeQF(QuaternionF A, QuaternionF B, float T){
        T = ClampF(T, 0, 1);
        
        float CosTheta = A.X*B.X + A.Y*B.Y + A.Z*B.Z + A.W*B.W;

        if(CosTheta < 0){
            B = new QuaternionF(-B.X, -B.Y, -B.Z, -B.W);
            CosTheta = -CosTheta;
        }

        if(CosTheta > 0.9995f){
            return new QuaternionF(
                LerpF(A.X, B.X, T),
                LerpF(A.Y, B.Y, T),
                LerpF(A.Z, B.Z, T),
                LerpF(A.W, B.W, T)
            ).Normalized;
        }

        float Angle = MathF.Acos(CosTheta);
        float SinTheta = MathF.Sin(Angle);

        float T1 = MathF.Sin((1 - T) * Angle) / SinTheta;
        float T2 = MathF.Sin(T * Angle) / SinTheta;

        return new QuaternionF(
            A.X*T1 + B.X*T2,
            A.Y*T1 + B.Y*T2,
            A.Z*T1 + B.Z*T2,
            A.W*T1 + B.W*T2
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3F SlerpSafeV3F(Vector3F A, Vector3F B, float T){
        T = ClampF(T, 0, 1);
        float Dot = Vector3F.Dot(A, B);
        Dot = ClampF(Dot, -1, 1);

        float Theta = MathF.Acos(Dot);
        Vector3F RelativeVector = (B - A * Dot).Normalize;

        return (A * MathF.Cos(Theta)) + (RelativeVector * MathF.Sin(Theta));
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