using WLO.Math;

namespace WLO.Render;

public struct UniformValue : IEquatable<UniformValue>{
    public int  Location;
    public DataType Type;

    public float    F1, F2, F3;
    public Matrix4F Matrix;
    
    public static UniformValue CreateF  (int Location, float    Value) => new UniformValue{ Type = DataType.Float, F1 = Value, Location = Location };
    public static UniformValue CreateI  (int Location, int      Value) => new UniformValue{ Type = DataType.Int, F1 = Value, Location = Location };
    public static UniformValue CreateV2F(int Location, Vector2F Value) => new UniformValue{ Type = DataType.Vector2F, F1 = Value.X, F2 = Value.Y, Location = Location };
    public static UniformValue CreateV3F(int Location, Vector3F Value) => new UniformValue{ Type = DataType.Vector3F, F1 = Value.X, F2 = Value.Y, F3 = Value.Z, Location = Location };
    public static UniformValue CreateM4F(int Location, Matrix4F Value) => new UniformValue{ Type = DataType.Matrix4F, Matrix = Value, Location = Location };

    public bool Equals(UniformValue Other){
        if(Location != Other.Location || Type != Other.Type){ return false; }
        return Type switch{
            DataType.Float    => F1 == Other.F1,
            DataType.Int      => (int)F1 == (int)Other.F1,
            DataType.Vector2F => F1 == Other.F1 && F2 == Other.F2,
            DataType.Vector3F => F1 == Other.F1 && F2 == Other.F2 && F3 == Other.F3,
            DataType.Matrix4F => Matrix.Equals(Other.Matrix),
            var _ => false
        };
    }

    public enum DataType : byte{
        Float,
        Int,
        Vector2F,
        Vector3F,
        Matrix4F
    }
}