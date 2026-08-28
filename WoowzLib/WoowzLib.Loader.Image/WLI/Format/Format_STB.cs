using System.Runtime.InteropServices;
using WLO;
using WLO.Math;

namespace WLI.Format;

public abstract class Format_STB : Format<Image>{
    public string LinkedID{ get; set; }
    
    public abstract bool __Is(byte[] Data);
    
    public Image __Load(byte[] Data){
        StbImageSharp.StbImage.stbi_set_flip_vertically_on_load(1);
        
        StbImageSharp.ImageResult ImageResult = StbImageSharp.ImageResult.FromMemory(Data, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
        Image Result = new Image(new Vector2I(ImageResult.Width, ImageResult.Height));

        Span<Color4B> PixelSpan = MemoryMarshal.Cast<byte, Color4B>(ImageResult.Data);
        PixelSpan.CopyTo(Result.Pixels);

        return Result;
    }
}