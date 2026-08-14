using WLO;

namespace WoowzLib.Core.WLO.Math;

public readonly struct Rect2I{
    public readonly Vector2I Position;
    public readonly Vector2I Size;

    public int X => Position.X;
    public int Y => Position.Y;
    public int W => Size.W;
    public int H => Size.H;

    public Rect2I(Vector2I Position, Vector2I Size){
        this.Position = Position;
        this.Size = Size;
    }

    public Rect2I(int X, int Y, int W, int H){
        Position = new Vector2I(X, Y);
        Size     = new Vector2I(W, H);
    }
}