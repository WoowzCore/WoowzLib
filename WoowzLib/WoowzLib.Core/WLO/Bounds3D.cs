using WLO.Math;

namespace WLO;

public readonly struct Bounds3D{
    public readonly Vector3F Min;
    public readonly Vector3F Max;
    
    /// Центральная точка бокса
    public readonly Vector3F Center;
    /// Размер бокса
    public readonly Vector3F Size;
    /// Радиус описанной сферы (Охватывает весь бокс целиком)
    public readonly float Radius;
    /// Радиус вписанной сферы (Максимальная сфера внутри бокса)
    public readonly float IRadius;

    public Bounds3D(Vector3F Min, Vector3F Max){
        this.Min = Min;
        this.Max = Max;

        Center  = (Min + Max) * 0.5f;
        Size    = Max - Min;
        Radius  = Size.Length * 0.5f;
        IRadius = System.MathF.Min(Size.X, System.MathF.Min(Size.Y, Size.Z)) * 0.5f;
    }
}