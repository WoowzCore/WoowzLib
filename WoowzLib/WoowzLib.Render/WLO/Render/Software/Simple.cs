using WLO.Math;

namespace WLO.Render.Software;

public class Simple : WLI_Render.Software{
    public void DrawPixel(Image Buffer, Vector2I Position, Color4B Color) => Buffer.SetPixel(Position, Color);
    
    public void DrawRect(Image Buffer, Rect2I Rect, Color4B Color){
        int StartX = System.Math.Max(0, Rect.X);
        int StartY = System.Math.Max(0, Rect.Y);
        int EndX   = System.Math.Min(Buffer.Size.W, Rect.X + Rect.W);
        int EndY   = System.Math.Min(Buffer.Size.H, Rect.Y + Rect.H);

        for(int Y = StartY; Y < EndY; Y++){
            int RowOffset = Y * Buffer.Size.W;
            for(int X = StartX; X < EndX; X++){
                Buffer[RowOffset + X] = Color;
            }
        }
    }
    
    public void DrawLine(Image Buffer, Vector2I Start, Vector2I End, Color4B Color){
        int X0 = Start.X;
        int Y0 = Start.Y;
        int X1 = End.X;
        int Y1 = End.Y;

        int DX = System.Math.Abs(X1 - X0);
        int SX = X0 < X1 ? 1 : -1;
        
        int DY = -System.Math.Abs(Y1 - Y0);
        int SY = Y0 < Y1 ? 1 : -1;

        int E = DX + DY;
        
        while(true){
            DrawPixel(Buffer, new Vector2I(X0, Y0), Color);

            if(X0 == X1 && Y0 == Y1){ break; }

            int E2 = E * 2;
            
            if(E2 >= DY){
                E += DY;
                X0 += SX;
            }

            if(E2 <= DX){
                E += DX;
                Y0 += SY;
            }
        }
    }

    public void Clear(Image Buffer, Color4B Color) => Buffer.Clear(Color);
}