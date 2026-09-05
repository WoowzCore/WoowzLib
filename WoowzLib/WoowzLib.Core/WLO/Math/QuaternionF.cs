using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace WLO.Math;

[StructLayout(LayoutKind.Explicit, Size = 16)]
public readonly struct QuaternionF : IEquatable<QuaternionF>, WLI.Packable{
    [FieldOffset(0)]
    private readonly Vector128<float> __Vector;

    public float X => __Vector.GetElement(0);
    public float Y => __Vector.GetElement(1);
    public float Z => __Vector.GetElement(2);
    public float W => __Vector.GetElement(3);

    public QuaternionF(float X, float Y, float Z, float W){
        __Vector = Vector128.Create(X, Y, Z, W);
    }

    public QuaternionF(Vector128<float> Vector){ __Vector = Vector; }

    public static QuaternionF Identity => new QuaternionF(0, 0, 0, 1);
    
    // ----------------------------------------------------------------------

    public Vector3F ToEuler(){
        float XX = X*X, YY = Y*Y, ZZ = Z*Z, WW = W * W;
        
        float M02 = 2 * (X * Z + Y * W);
        
        Vector3F Angles = new Vector3F();

        if(MathF.Abs(M02) < 0.99999f){
            Angles.Pitch = MathF.Atan2(-(2 * (Y*Z - X*W)), WW - XX - YY + ZZ);
            Angles.Yaw   = MathF.Asin(M02);
            Angles.Roll  = MathF.Atan2(-(2 * (X*Y - Z*W)), WW + XX - YY - ZZ);
        }else{
            Angles.Pitch = MathF.Atan2(2 * (X*Y + Z*W), WW - XX + YY - ZZ);
            Angles.Yaw   = MathF.PI * 0.5f * (M02 >= 1 ? 1 : -1);
            Angles.Roll  = 0;
        }

        return Angles;
    }

    public static QuaternionF FromEuler(Vector3F Angles) {
        float HP = Angles.Pitch * 0.5f;
        float HY = Angles.Yaw   * 0.5f;
        float HR = Angles.Roll  * 0.5f;

        float CP = MathF.Cos(HP); float SP = MathF.Sin(HP);
        float CY = MathF.Cos(HY); float SY = MathF.Sin(HY);
        float CR = MathF.Cos(HR); float SR = MathF.Sin(HR);

        return new QuaternionF(
            SP*CY*CR + CP*SY*SR,
            CP*SY*CR - SP*CY*SR,
            CP*CY*SR + SP*SY*CR,
            CP*CY*CR - SP*SY*SR
        );
    }
    
    // ----------------------------------------------------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static QuaternionF operator *(QuaternionF A, QuaternionF B) => new QuaternionF(
        A.W * B.X + A.X * B.W + A.Y * B.Z - A.Z * B.Y,
        A.W * B.Y - A.X * B.Z + A.Y * B.W + A.Z * B.X,
        A.W * B.Z + A.X * B.Y - A.Y * B.X + A.Z * B.W,
        A.W * B.W - A.X * B.X - A.Y * B.Y - A.Z * B.Z   
    );

    public float Length => System.MathF.Sqrt(Vector128.Dot(__Vector, __Vector));

    public QuaternionF Normalized{
        get{
            float L = Length;
            if(L < 1e-6f){ return Identity; }
            return new QuaternionF(Vector128.Divide(__Vector, Vector128.Create(L)));
        }
    }

    public QuaternionF Conjugate => new QuaternionF(-X, -Y, -Z, W);
    
    // ----------------------------------------------------------------------

    public Matrix4F ToMatrix4F(){
        float XX = X*X, YY = Y*Y, ZZ = Z*Z;
        float YX = Y*X, ZY = Z*Y, XZ = X*Z;
        float WX = W*X, WY = W*Y, WZ = W*Z;

        return new Matrix4F(
            1 - 2 * (YY + ZZ), 2 * (YX + WZ), 2 * (XZ - WY), 0,
            2 * (YX - WZ), 1 - 2 * (XX + ZZ), 2 * (ZY + WX), 0,
            2 * (XZ + WY), 2 * (ZY - WX), 1 - 2 * (XX + YY), 0,
            0, 0, 0, 1
        );
    }
    
    // ----------------------------------------------------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator System.Numerics.Quaternion(QuaternionF Q) => new Quaternion(Q.X, Q.Y, Q.Z, Q.W);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator QuaternionF(System.Numerics.Quaternion Q) => new QuaternionF(Q.X, Q.Y, Q.Z, Q.W);

    public bool Equals(QuaternionF Other) => __Vector.Equals(Other.__Vector);
    public override bool Equals(object? Object) => Object is QuaternionF Other && Equals(Other);
    public override int GetHashCode() => __Vector.GetHashCode();

    public static bool operator ==(QuaternionF L, QuaternionF R) =>  L.Equals(R);
    public static bool operator !=(QuaternionF L, QuaternionF R) => !L.Equals(R);
    
    // ----------------------------------------------------------------------

    public Dictionary<string, object?> __Pack() => new Dictionary<string, object?>{
        ["XYZW"] = $"{X}|{Y}|{Z}|{W}"
    };

    public void __Unpack(Dictionary<string, object?> Data){
        string XYZW = WL.Packer.Get(Data, "XYZW", "0|0|0|1")!;
        string[] P = XYZW.Split('|');
        if(P.Length >= 4){
            float.TryParse(P[0], out float X);
            float.TryParse(P[1], out float Y);
            float.TryParse(P[2], out float Z);
            float.TryParse(P[3], out float W);
            Unsafe.AsRef(in __Vector) = Vector128.Create(X, Y, Z, W);
        }
    }
}