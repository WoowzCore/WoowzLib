using WLI.Format;

namespace WLO.Loader;

public class TGA : Format_STB{
    public const string ID = "TGA";
    
    static TGA() => WL.Loader.Register(ID, new TGA());
    
    public override bool __Is(byte[] Data){
        if(Data.Length < 18){ return false; }
        return Data[2] <= 11;
    }
    
    // ----------------------------------------------------------------------

    public static Image Load(byte[] Data) => WL.Loader.Load<Image>(ID, Data);
    public static bool Is(byte[] Data) => WL.Loader.Is<Image>(ID, Data);
}