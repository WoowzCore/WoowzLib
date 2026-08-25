using System.Runtime.InteropServices;
using WLO.Math;

namespace WoowzLib.Render.WLO;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Vertex{
    public Vector3F Position;
    public Color4B  Color;
    
    // TODO, сделать бессконечное кол-во переменных что-бы можно было писать не только позицию и цвет

    public Vertex(Vector3F Position, Color4B Color){
        this.Position = Position;
        this.Color = Color;
    }
    
    public Vertex(Vector2F Position, Color4B Color){
        this.Position = new Vector3F(Position.X, Position.Y, 0);
        this.Color = Color;
    }
}