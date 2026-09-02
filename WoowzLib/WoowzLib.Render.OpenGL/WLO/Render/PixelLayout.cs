namespace WLO.Render;

public class PixelLayout{
    public PixelAttribute[] Attributes{ get; }

    public PixelLayout(params PixelAttribute[] Attributes){
        this.Attributes = Attributes;
    }
}