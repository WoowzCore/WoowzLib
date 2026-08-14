using WLO;
using WoowzLib.Core.WLO.Math;

namespace WoowzLib.Core.WLO;

public class FrameBuffer{
    public readonly Vector2I  Size;
    public readonly Color4B[] Pixels;

    public FrameBuffer(Vector2I Size){
        this.Size = Size;
        Pixels = new Color4B[Size.W * Size.H];
    }

    public void Clear(Color4B Color){
        Array.Fill(Pixels, Color);
    }

    public bool Out(Vector2I Position) => !(Position.X >= 0 && Position.X < Size.W && Position.Y >= 0 && Position.Y < Size.H);

    public void SetPixel(Vector2I Position, Color4B Color){
        if(!Out(Position)){
            Pixels[Position.Y * Size.W + Position.X] = Color;
        }
    }
}