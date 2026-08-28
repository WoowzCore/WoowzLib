using WLO.Math;

namespace WLO;

public struct Bounds{
    public Vector3F Min;
    public Vector3F Max;
    public Vector3F Center => (Min + Max) * 0.5f;
    public Vector3F Size => Max - Min;
    // todo, описанная сфера (нужно ещё вписанную сделать будет)
    public float Radius => Size.Length * 0.5f;

    public Bounds(Vector3F Min, Vector3F Max){
        this.Min = Min;
        this.Max = Max;
    }
}