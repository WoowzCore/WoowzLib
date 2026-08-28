using WLI.Format;

namespace WLO.Loader;

public class JPG : Format_STB{
    public const string ID = "JPG";
    
    static JPG() => WL.Loader.Register(ID, new JPG());
    
    public override bool __Is(byte[] Data){
        if(Data.Length < 3){ return false; }
        return Data[0] == 0xFF && Data[1] == 0xD8 && Data[2] == 0xFF;
    }
    
    // ----------------------------------------------------------------------

    public static Image Load(byte[] Data) => WL.Loader.Load<Image>(ID, Data);
    public static bool Is(byte[] Data) => WL.Loader.Is<Image>(ID, Data);
}