using System.Runtime.InteropServices;
using WLI.Format;
using WLO.Math;

namespace WLO.Loader;

public class PNG : ImageFormat{
    static PNG(){
        WL.Loader.Register("PNG", new PNG());
    }
    
    public bool __Is(byte[] Data){
        if(Data.Length < 8){ return false; }
        return Data[0] == 0x89 && Data[1] == 0x50 && Data[2] == 0x4E && Data[3] == 0x47 && Data[4] == 0x0D && Data[5] == 0x0A && Data[6] == 0x1A && Data[7] == 0x0A;
    }

    public Image __Load(byte[] Data){
        StbImageSharp.StbImage.stbi_set_flip_vertically_on_load(1);
        
        StbImageSharp.ImageResult ImageResult = StbImageSharp.ImageResult.FromMemory(Data, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
        Image Result = new Image(new Vector2I(ImageResult.Width, ImageResult.Height));

        Span<Color4B> PixelSpan = MemoryMarshal.Cast<byte, Color4B>(ImageResult.Data);
        PixelSpan.CopyTo(Result.Pixels);

        return Result;
    }
    
    // ----------------------------------------------------------------------

    public static Image Load(byte[] Data) => WL.Loader.LoadImage("PNG", Data);
    public static bool Is(byte[] Data) => WL.Loader.IsImage("PNG", Data);
}