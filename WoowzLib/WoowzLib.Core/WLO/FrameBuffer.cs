using System.Runtime.CompilerServices;
using WLO.Math;

namespace WLO;

public class FrameBuffer{
    public readonly Vector2I  Size;
    public readonly Color4B[] Pixels;

    public FrameBuffer(Vector2I Size, Color4B StartColor) : this(Size) => Clear(StartColor);
    
    public FrameBuffer(Vector2I Size){
        this.Size = Size;
        Pixels = new Color4B[Size.W * Size.H];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Out(Vector2I Position) => !(Position.X >= 0 && Position.X < Size.W && Position.Y >= 0 && Position.Y < Size.H);

    public void SetPixel(Vector2I Position, Color4B Color){
        if(!Out(Position)){
            Pixels[Position.Y * Size.W + Position.X] = Color;
        }
    }

    /**
     * Заполняет буфер полностью указанным цветом
     * Все пиксели будут этим цветом
     */
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear(Color4B Color) => Array.Fill(Pixels, Color);

    public Color4B this[int X, int Y]{
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Pixels[Y * Size.W + X];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => Pixels[Y * Size.W + X] = value;
    }

    public Color4B this[Vector2I Position]{
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this[Position.X, Position.Y];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => this[Position.X, Position.Y] = value;
    }

    public Color4B this[int I]{
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Pixels[I];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => Pixels[I] = value;
    }
}