using System.Runtime.InteropServices;
using WLO.Math;

namespace WoowzLib.Render.WLO;

[StructLayout(LayoutKind.Sequential)]
public struct Vertex{
    public Vector2F Position;
    public Color4B  Color;
    
    // TODO, сделать бессконечное кол-во переменных что-бы можно было писать не только позицию и цвет

    public Vertex(Vector2F Position, Color4B Color){
        this.Position = Position;
        this.Color = Color;
    }
}