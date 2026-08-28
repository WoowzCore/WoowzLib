using WLI.Format;

namespace WLO.Loader;

public class BMP : Format_STB{
    public const string ID = "BMP";
    
    static BMP() => WL.Loader.Register(ID, new BMP());
    
    public override bool __Is(byte[] Data){
        if(Data.Length < 2){ return false; }
        return Data[0] == 0x42 && Data[1] == 0x4D;
    }
    
    // ----------------------------------------------------------------------

    public static Image Load(byte[] Data) => WL.Loader.Load<Image>(ID, Data);
    public static bool Is(byte[] Data) => WL.Loader.Is<Image>(ID, Data);
}