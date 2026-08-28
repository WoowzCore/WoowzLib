using System.Runtime.InteropServices;
using WLO.Math;

namespace WLO;

// TODO, сделать бессконечное кол-во переменных что-бы можно было писать не только позицию и цвет
// todo, сделать уникальным или интерфейсом

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Vertex{
    public Vector3F Position;
    public Vector3F Normal;
    public Vector2F UV;
    public Color4B  Color;
    public uint     ID;
    
    public Vertex(Vector3F Position = default, Vector2F UV = default, Vector3F Normal = default, Color4B Color = default, uint ID = 0){
        this.Position = Position;
        this.UV = UV;
        this.Normal = Normal;
        this.Color = Color;
        this.ID = ID;
    }
}